using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using System.IO;
using log4net;
using System.Linq;
using ZedGraph; // Graphs
using System.Xml;
using System.Collections;
using System.Text.RegularExpressions;
using System.Threading;
//using MissionPlanner.Controls;
using GMap.NET;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
//using MissionPlanner.Utilities;

namespace AutelXSPLogViewer
{
    public partial class LogBrowse : Form
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        DataTable m_dtCSV = new DataTable();

        CollectionBuffer logdata;
        Hashtable logdatafilter = new Hashtable();
        Hashtable seenmessagetypes = new Hashtable();

        List<TextObj> ModeCache = new List<TextObj>();
        List<TextObj> ErrorCache = new List<TextObj>();
        List<TextObj> TimeCache = new List<TextObj>();

        const int typecoloum = 2;

        List<PointPairList> listdata = new List<PointPairList>();
        GMapOverlay mapoverlay;
        GMapOverlay markeroverlay;
        LineObj m_cursorLine = null;
        Hashtable dataModifierHash = new Hashtable();

        DFLog dflog = new DFLog();

        public string logfilename;

        private bool readmavgraphsxml_runonce = false;

        class DataModifer
        {
            private readonly bool isValid;
            public readonly string commandString;
            public double offset = 0;
            public double scalar = 1;
            public bool doOffsetFirst = false;

            public DataModifer()
            {
                this.commandString = "";
                this.isValid = false;
            }

            public DataModifer(string _commandString)
            {
                this.commandString = _commandString;
                this.isValid = ParseCommandString(_commandString);
            }

            private bool ParseCommandString(string _commandString)
            {
                if (_commandString == null)
                {
                    return false;
                }

                char[] splitOnThese = { ' ', ',' };
                string[] split = _commandString.Trim().Split(splitOnThese, 2, StringSplitOptions.RemoveEmptyEntries);

                if (split.Length < 1)
                {
                    return false;
                }

                for (int i = 0; i < split.Length; i++)
                {
                    string strTrimmed = split[i].Trim();

                    // each command is a minimum of 2 chars
                    // expecting: x123, /5, +1000, *10, *0.01, -50,
                    if (strTrimmed.Length < 2)
                    {
                        return false;
                    }

                    char cmd = strTrimmed[0];
                    string param = strTrimmed.Substring(1);
                    double value = 0;

                    if (double.TryParse(param, out value) == false)
                    {
                        return false;
                    }

                    switch (cmd)
                    {
                        case 'x':
                        case '*':
                            this.scalar = value;
                            break;
                        case '\\':
                        case '/':
                            this.scalar = 1.0 / value;
                            break;

                        case '+':
                            this.doOffsetFirst = (i == 0);
                            this.offset = value;
                            break;
                        case '-':
                            this.doOffsetFirst = (i == 0);
                            this.offset = -value;
                            break;

                        default:
                            return false;
                    } // switch
                } // for i
                return true;
            }

            public bool IsValid()
            {
                return this.isValid;
            }

            public static string GetNodeName(string parent, string child)
            {
                return parent + "." + child;
            }
        }


        class displayitem
        {
            public string type;
            public string field;
            public string expression;
            public bool left = true;
        }

        class displaylist
        {
            public string Name;
            public displayitem[] items;

            public override string ToString()
            {
                return Name;
            }
        }

        List<displaylist> graphs = new List<displaylist>()
        {
            new displaylist() {Name = "None"},
            new displaylist()
            {
                Name = "Altitude",
                items = new displayitem[]
                {
                    new displayitem() {type = "GPS", field = "Alt"},
                    new displayitem() {type = "GPOS", field = "Alt"}
                }
            },
            new displaylist()
            {
                Name = "Roll",
                items = new displayitem[]
                {
                    new displayitem() {type = "ATT", field = "Roll"},
                    new displayitem() {type = "ATTC", field = "Roll"},
                    new displayitem() {type = "RC", field = "Ch0"}
                }
            },
            new displaylist()
            {
                Name = "Pitch",
                items = new displayitem[]
                {
                    new displayitem() {type = "ATT", field = "Pitch"},
                    new displayitem() {type = "ATTC", field = "Pitch"},
                    new displayitem() {type = "RC", field = "Ch1"}
                }
            },
            new displaylist()
            {
                Name = "Yaw",
                items = new displayitem[]
                {
                    new displayitem() {type = "ATT", field = "Yaw"},
                    new displayitem() {type = "ATTC", field = "Yaw"},
                    new displayitem() {type = "RC", field = "Ch2"}
                }
            },
            /*
            new displaylist()
            {
                Name = "Mechanical Failure",
                items = new displayitem[]
                {
                    new displayitem() {type = "ATT", field = "Roll"},
                    new displayitem() {type = "ATT", field = "DesRoll"},
                    new displayitem() {type = "ATT", field = "Pitch"},
                    new displayitem() {type = "ATT", field = "DesPitch"},
                    new displayitem() {type = "CTUN", field = "Alt", left = false},
                    new displayitem() {type = "CTUN", field = "DAlt", left = false}
                }
            },
            new displaylist()
            {
                Name = "Mechanical Failure - Stab",
                items =
                    new displayitem[]
                    {
                        new displayitem() {type = "ATT", field = "Roll"},
                        new displayitem() {type = "ATT", field = "DesRoll"}
                    }
            },
            new displaylist()
            {
                Name = "Mechanical Failure - Auto",
                items =
                    new displayitem[]
                    {
                        new displayitem() {type = "ATT", field = "Roll"},
                        new displayitem() {type = "NTUN", field = "DRoll"}
                    }
            }, */
            new displaylist()
            {
                Name = "Vibrations",
                items =
                    new displayitem[]
                    {
                        new displayitem() {type = "IMU", field = "AccX"},
                        new displayitem() {type = "IMU", field = "AccY"},
                        new displayitem() {type = "IMU", field = "AccZ"}
                    }
            },
            /*
            new displaylist()
            {
                Name = "Vibrations 3.3",
                items = new displayitem[]
                {
                    new displayitem() {type = "VIBE", field = "VibeX"},
                    new displayitem() {type = "VIBE", field = "VibeY"},
                    new displayitem() {type = "VIBE", field = "VibeZ"}
                    , new displayitem() {type = "VIBE", field = "Clip0", left = false},
                    new displayitem() {type = "VIBE", field = "Clip1", left = false},
                    new displayitem() {type = "VIBE", field = "Clip2", left = false}
                }
            },
            */
            new displaylist()
            {
                Name = "GPS Availability",
                items =
                    new displayitem[]
                    {
                        new displayitem() {type = "GPS", field = "Fix"},
                        new displayitem() {type = "GPS", field = "nSat", left = false}
                    }
            },
            new displaylist()
            {
                Name = "Power - Servo",
                items = new displayitem[] 
                {
                    new displayitem() {type = "PWR", field = "Servo5V" },
                    new displayitem() {type = "PWR", field = "ServoOk" }
                    }
                },
            /*
            new displaylist()
            {
                Name = "Errors",
                items = new displayitem[] {new displayitem() {type = "ERR", field = "ECode"}}
            },
            */
            new displaylist()
            {
                Name = "Battery - Throttle to Current",
                items =
                    new displayitem[]
                    {
                        new displayitem() {type = "RC", field = "Ch3"},
                        new displayitem() {type = "BATT", field = "Curr", left = false}
                    }
            },
            /*
            new displaylist()
            {
                Name = "imu consistency xyz",
                items = new displayitem[]
                {
                    new displayitem() {type = "IMU", field = "AccX"},
                    new displayitem() {type = "IMU2", field = "AccX"},
                    new displayitem() {type = "IMU", field = "AccY"},
                    new displayitem() {type = "IMU2", field = "AccY"},
                    new displayitem() {type = "IMU", field = "AccZ", left = false},
                    new displayitem() {type = "IMU2", field = "AccZ", left = false},
                }
            },
            new displaylist()
            {
                Name = "mag consistency xyz",
                items = new displayitem[]
                {
                    new displayitem() {type = "MAG", field = "MagX"},
                    new displayitem() {type = "MAG2", field = "MagX"},
                    new displayitem() {type = "MAG", field = "MagY", left = false},
                    new displayitem() {type = "MAG2", field = "MagY", left = false},
                    new displayitem() {type = "MAG", field = "MagZ"},
                    new displayitem() {type = "MAG2", field = "MagZ"},
                }
            },
            new displaylist()
            {
                Name = "copter loiter",
                items = new displayitem[]
                {
                    new displayitem() {type = "NTUN", field = "DVelX"},
                    new displayitem() {type = "NTUN", field = "VelX"},
                    new displayitem() {type = "NTUN", field = "DVelY"},
                    new displayitem() {type = "NTUN", field = "VelY"},
                }
            },
            */
            new displaylist()
            {
                Name = "Alt Hold",
                items = new displayitem[]
                {
                    new displayitem() {type = "SENS", field = "BaroAlt"},
                    new displayitem() {type = "GPS", field = "Alt"}
                }
            }
            /*,
            new displaylist()
            {
                Name = "ekf VEL tune",
                items = new displayitem[]
                {
                    new displayitem() {type = "EKF3", field = "IVN"},
                    new displayitem() {type = "EKF3", field = "IPN"},
                    new displayitem() {type = "EKF3", field = "IVE"},
                    new displayitem() {type = "EKF3", field = "IPE"},
                    new displayitem() {type = "EKF3", field = "IVD"},
                    new displayitem() {type = "EKF3", field = "IPD"},
                }
            },
            */
        };

        /*  
    105    +Format characters in the format string for binary log messages  
    106    +  b   : int8_t  
    107    +  B   : uint8_t  
    108    +  h   : int16_t  
    109    +  H   : uint16_t  
    110    +  i   : int32_t  
    111    +  I   : uint32_t  
    112    +  f   : float  
    113    +  N   : char[16]  
    114    +  c   : int16_t * 100  
    115    +  C   : uint16_t * 100  
    116    +  e   : int32_t * 100  
    117    +  E   : uint32_t * 100  
    118    +  L   : uint32_t latitude/longitude  
    119    + */

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.G))
            {
                string lineno = "0";
                InputBox.Show("Line no", "Enter Line Number", ref lineno);

                int line = int.Parse(lineno);

                try
                {
                    dataGridView1.CurrentCell = dataGridView1[1, line - 1];
                }
                catch
                {
                    CustomMessageBox.Show("Line Doesn't Exist");
                }

                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        public LogBrowse()
        {
            InitializeComponent();

            // config map      
            log.Info("Map Setup");
            gMapControl1.CacheLocation = Path.GetDirectoryName(Application.ExecutablePath) + Path.DirectorySeparatorChar +
                                         "gmapcache" + Path.DirectorySeparatorChar;

            gMapControl1.MapProvider = GMap.NET.MapProviders.GoogleSatelliteMapProvider.Instance;
            
            gMapControl1.MinZoom = 0;
            gMapControl1.MaxZoom = 24;
            gMapControl1.Zoom = 3;
            gMapControl1.Position = new PointLatLng(41.850033, -87.6500523);
  
            //gMapControl1.OnMapZoomChanged += gMapControl1_OnMapZoomChanged;

            gMapControl1.DisableFocusOnMouseEnter = true;


            gMapControl1.RoutesEnabled = true;
            gMapControl1.PolygonsEnabled = true;


            GMapOverlay tfrpolygons = new GMapOverlay("tfrpolygons");
            gMapControl1.Overlays.Add(tfrpolygons);

            GMapOverlay kmlpolygons = new GMapOverlay("kmlpolygons");
            gMapControl1.Overlays.Add(kmlpolygons);

            GMapOverlay geofence = new GMapOverlay("geofence");
            gMapControl1.Overlays.Add(geofence);

            GMapOverlay polygons = new GMapOverlay("polygons");
            gMapControl1.Overlays.Add(polygons);

            GMapOverlay photosoverlay = new GMapOverlay("photos overlay");
            gMapControl1.Overlays.Add(photosoverlay);

            GMapOverlay routes = new GMapOverlay("routes");
            gMapControl1.Overlays.Add(routes);

            GMapOverlay rallypointoverlay = new GMapOverlay("rally points");
            gMapControl1.Overlays.Add(rallypointoverlay);

            //GMapOverlay gMapControl1.Overlays.Add(poioverlay);
            
            mapoverlay = new GMapOverlay("overlay");
            markeroverlay = new GMapOverlay("markers");
            gMapControl1.Overlays.Add(mapoverlay);
            gMapControl1.Overlays.Add(markeroverlay);
            //chk_time.Checked = true;

            dataGridView1.RowUnshared += dataGridView1_RowUnshared;

            //MissionPlanner.Utilities.Tracking.AddPage(this.GetType().ToString(), this.Text);
        }

        public class graphitem
        {
            public string name;
            public List<string> expressions = new List<string>();
            public string description;
        }

        private void readmavgraphsxml()
        {
            if (readmavgraphsxml_runonce)
                return;

            readmavgraphsxml_runonce = true;

            List<graphitem> items = new List<graphitem>();
            
            using (
                XmlReader reader =
                    XmlReader.Create(Application.StartupPath + Path.DirectorySeparatorChar + "mavgraphs.xml"))
            {
                while (reader.Read())
                {
                    if (reader.ReadToFollowing("graph"))
                    {
                        graphitem newGraphitem = new graphitem();

                        for (int a = 0; a < reader.AttributeCount; a++)
                        {
                            reader.MoveToAttribute(a);
                            if (reader.Name.ToLower() == "name")
                            {
                                newGraphitem.name = reader.Value;
                            }
                        }

                        reader.MoveToElement();

                        XmlReader inner = reader.ReadSubtree();

                        while (inner.Read())
                        {
                            if (inner.IsStartElement())
                            {
                                if (inner.Name.ToLower() == "expression")
                                    newGraphitem.expressions.Add(inner.ReadString().Trim());
                                else if (inner.Name.ToLower() == "description")
                                    newGraphitem.description = inner.ReadString().Trim();
                            }
                        }

                        processGraphItem(newGraphitem);

                        items.Add(newGraphitem);
                    }
                }
            }
        }

        void processGraphItem(graphitem graphitem)
        {
            List<displayitem> list = new List<displayitem>();

            foreach (var expression in graphitem.expressions)
            {
                var items = expression.Split(new char[] { ' ', '\t', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var item in items)
                {
                    var matchs = Regex.Matches(item.Trim(), @"^([A-z0-9_]+)\.([A-z0-9_]+)[:2]*$");

                    if (matchs.Count > 0)
                    {
                        foreach (Match match in matchs)
                        {
                            var temp = new displayitem();
                            // right axis
                            if (item.EndsWith(":2"))
                                temp.left = false;

                            temp.type = match.Groups[1].Value.ToString();
                            temp.field = match.Groups[2].Value.ToString();

                            list.Add(temp);
                        }
                    }
                    else
                    {
                        var temp = new displayitem();
                        if (item.EndsWith(":2"))
                            temp.left = false;
                        temp.expression = item;
                        temp.type = item;
                        list.Add(temp);
                    }
                }
            }

            var dispitem = new displaylist()
            {
                Name = graphitem.name,
                items = list.ToArray()
            };

            graphs.Add(dispitem);
        }

        void dataGridView1_RowUnshared(object sender, DataGridViewRowEventArgs e)
        {
        }

        private void LogBrowse_Load()
        {
            mapoverlay.Clear();
            markeroverlay.Clear();

            logdatafilter.Clear();

            m_dtCSV.Clear();

            if (logdata != null)
                logdata.Clear();

            GC.Collect();

            ErrorCache = new List<TextObj>();
            ModeCache = new List<TextObj>();
            TimeCache = new List<TextObj>();

            seenmessagetypes = new Hashtable();

            if (!File.Exists(logfilename))
            {
                using (OpenFileDialog openFileDialog1 = new OpenFileDialog())
                {
                    openFileDialog1.Filter = "Log Files|*.log;*.bin";
                    openFileDialog1.FilterIndex = 2;
                    openFileDialog1.RestoreDirectory = true;
                    openFileDialog1.Multiselect = false;

                    //TODO
                    //openFileDialog1.InitialDirectory = Settings.Instance.LogDir;

                    if (openFileDialog1.ShowDialog() == DialogResult.OK)
                    {

                        // load file
                        this.Cursor = Cursors.WaitCursor;
                        logfilename = openFileDialog1.FileName;
                        this.progressBar1.Step = 1;
                        this.progressBar1.Value = 0;
                        this.progressBar1.Maximum = 100;
                        this.lblProgMsg.Text = "loading file: "+Path.GetFileName(openFileDialog1.FileName)+"...";
                        this.lblProgMsg.Refresh();

                        ThreadPool.QueueUserWorkItem(o => {
                            LoadLog(logfilename);

                            progressBar1.Invoke(new MethodInvoker(() => this.progressBar1.Value = 0));
                            lblProgMsg.Invoke(new MethodInvoker(() => {
                                lblProgMsg.Text = "";
                                lblProgMsg.Refresh();
                            }));
                        this.Invoke(new MethodInvoker(() =>
                           {
                               Cursor = Cursors.Default;
                               saveAsToolStripMenuItem.Enabled = true;
                           }));

                    
                          });
                    }
                    else
                    {
                        this.Close();
                        return;
                    }
                }
            }
            else
            {
                ThreadPool.QueueUserWorkItem(o => LoadLog(logfilename));
            }


        }

        public void LoadLog(string FileName)
        {
            Loading.ShowLoading(Strings.Scanning_File, this);

            try
            {
                Stream stream;

                stream = File.Open(FileName, FileMode.Open, FileAccess.Read, FileShare.Read);

                log.Info("before read " + (GC.GetTotalMemory(false) / 1024.0 / 1024.0));

                logdata = new CollectionBuffer(stream);

                log.Info("got log lines " + (GC.GetTotalMemory(false) / 1024.0 / 1024.0));

                log.Info("about to create DataTable " + (GC.GetTotalMemory(false) / 1024.0 / 1024.0));
                m_dtCSV = new DataTable();

                log.Info("process to datagrid " + (GC.GetTotalMemory(false) / 1024.0 / 1024.0));

                Loading.ShowLoading("Scanning coloum widths", this);

                int b = 0;
                double per = 0;
                int lastper = 0;
                double cnt = (double) logdata.Count;
                foreach (var item2 in logdata)
                {
                    b++;
                    per = (b / cnt) * 100.0f;
                    if ((int) per != lastper)
                    {
                        lastper = (int) per;
                        progressBar1.Invoke(new MethodInvoker(() => progressBar1.Value = lastper));
                    }
                    var item = dflog.GetDFItemFromLine(item2, b);

                    if (item.items != null)
                    {
                        while (m_dtCSV.Columns.Count < (item.items.Length + typecoloum))
                        {
                            m_dtCSV.Columns.Add();
                        }

                        seenmessagetypes[item.msgtype] = "";

                        // check first 1000000 lines for max coloums needed
                        if (b > 1000000)
                            break;
                    }
                }

                log.Info("Done " + (GC.GetTotalMemory(false) / 1024.0 / 1024.0));

                this.Invoke((Action)delegate
                {
                    LoadLog2(FileName, logdata);
                });
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("Failed to read File: " + ex.ToString());
                return;
            }
        }

        void LoadLog2(String FileName, CollectionBuffer logdata)
        {
            try
            {
                this.Text = "X-Star Log Viewer - " + Path.GetFileName(FileName);

                log.Info("set dgv datasourse " + (GC.GetTotalMemory(false) / 1024.0 / 1024.0));


                dataGridView1.VirtualMode = true;
                dataGridView1.RowCount = 0;
                dataGridView1.RowCount = logdata.Count;
                dataGridView1.ColumnCount = m_dtCSV.Columns.Count;

                log.Info("datagrid size set " + (GC.GetTotalMemory(false) / 1024.0 / 1024.0));


                log.Info("datasource set " + (GC.GetTotalMemory(false) / 1024.0 / 1024.0));
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("Failed to read File: " + ex.ToString());
                return;
            }

            foreach (DataGridViewColumn column in dataGridView1.Columns)
            {
                column.SortMode = DataGridViewColumnSortMode.NotSortable;
            }

            log.Info("Done timetable " + (GC.GetTotalMemory(false) / 1024.0 / 1024.0));

            Loading.ShowLoading("Generating Map/Time", this);

            DrawMap();

            log.Info("Done map " + (GC.GetTotalMemory(false) / 1024.0 / 1024.0));

            try
            {
                DrawTime();
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }

            log.Info("Done time " + (GC.GetTotalMemory(false) / 1024.0 / 1024.0));

            CreateChart(zg1);

            ResetTreeView(seenmessagetypes);

            Loading.Close();

            if (dflog.logformat.Count == 0)
            {
                CustomMessageBox.Show(Strings.WarningLogBrowseFMTMissing, Strings.ERROR);
                this.Close();
                return;
            }

            // update preselection graphs
            //readmavgraphsxml();

            //CMB_preselect.DisplayMember = "Name";
            CMB_preselect.DataSource = null;
            CMB_preselect.DataSource = graphs;
        }

        private void UntickTreeView()
        {
            foreach (TreeNode node1 in treeView1.Nodes)
            {
                if (node1.Checked)
                    node1.Checked = false;
                foreach (TreeNode node2 in node1.Nodes)
                {
                    if (node2.Checked)
                        node2.Checked = false;
                    foreach (TreeNode node3 in node2.Nodes)
                    {
                        if (node3.Checked)
                            node3.Checked = false;
                    }
                }
            }
        }

        private void ResetTreeView(Hashtable seenmessagetypes)
        {
            treeView1.Nodes.Clear();
            dataModifierHash = new Hashtable();

            var sorted = new SortedList(dflog.logformat);

            foreach (DFLog.Label item in sorted.Values)
            {
                TreeNode tn = new TreeNode(item.Name);

                if (seenmessagetypes.ContainsKey(item.Name))
                {
                    treeView1.Nodes.Add(tn);
                    foreach (var item1 in item.FieldNames)
                    {
                        tn.Nodes.Add(item1);
                    }
                }
            }
        }

        private void dataGridView1_RowEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridView1.ColumnCount < typecoloum)
                return;

            try
            {
                // number the coloums
                int a = -typecoloum;
                foreach (DataGridViewColumn col in dataGridView1.Columns)
                {
                    col.HeaderText = a.ToString();
                    a++;
                }
            }
            catch
            {
            }
            try
            {
                // process the line type
                string option = dataGridView1[typecoloum, e.RowIndex].EditedFormattedValue.ToString();

                // new self describing log
                if (dflog.logformat.ContainsKey(option))
                {
                    int a = typecoloum + 1;
                    foreach (string name in dflog.logformat[option].FieldNames)
                    {
                        dataGridView1.Columns[a].HeaderText = name;
                        a++;
                    }
                    for (; a < dataGridView1.Columns.Count; a++)
                    {
                        dataGridView1.Columns[a].HeaderText = "";
                    }

                }

                return;
            }
            catch
            {
                log.Info("DGV logbrowse error");
            }
        }

                /*
                if (option.StartsWith("PID-"))
                    option = "PID-1";

                using (
                    XmlReader reader =
                        XmlReader.Create(Path.GetDirectoryName(Application.ExecutablePath) + Path.DirectorySeparatorChar +
                                         "dataflashlog.xml"))
                {
                    reader.Read();
                    reader.ReadStartElement("LOGFORMAT");

                    
                    if (MainV2.comPort.MAV.cs.firmware == MainV2.Firmwares.ArduPlane)
                    {
                        reader.ReadToFollowing("APM");
                    }
                    else if (MainV2.comPort.MAV.cs.firmware == MainV2.Firmwares.ArduRover)
                    {
                        reader.ReadToFollowing("APRover");
                    }
                    else
                    {
                        reader.ReadToFollowing("AC2");
                    }
                    
                    reader.ReadToFollowing(option);

                    dataGridView1.Columns[1].HeaderText = "";

                    if (reader.NodeType == XmlNodeType.None)
                        return;

                    XmlReader inner = reader.ReadSubtree();

                    inner.MoveToElement();

                    int a = 2;

                    while (inner.Read())
                    {
                        inner.MoveToElement();
                        if (inner.IsStartElement())
                        {
                            if (inner.Name.StartsWith("F"))
                            {
                                dataGridView1.Columns[a].HeaderText = inner.ReadString();
                                log.Info(a + " " + dataGridView1.Columns[a].HeaderText);
                                a++;
                            }
                        }
                    }

                    for (; a < dataGridView1.Columns.Count; a++)
                    {
                        dataGridView1.Columns[a].HeaderText = "";
                    }
                }
            }

            */

        Color[] colours = new Color[]
        {
            Color.Red,
            Color.Green,
            Color.Blue,
            Color.Pink,
            Color.Yellow,
            Color.Orange,
            Color.Violet,
            Color.Wheat,
            Color.Teal,
            Color.Silver
        };

        public void CreateChart(ZedGraphControl zgc)
        {
            GraphPane myPane = zgc.GraphPane;

            // Set the titles and axis labels
            myPane.Title.Text = "Value Graph";
            myPane.XAxis.Title.Text = "Line Number";
            myPane.YAxis.Title.Text = "Output";

            // Show the x axis grid
            myPane.XAxis.MajorGrid.IsVisible = true;

            //myPane.XAxis.Scale.Min = 0;
            //myPane.XAxis.Scale.Max = -1;

            // Make the Y axis scale red
            myPane.YAxis.Scale.FontSpec.FontColor = Color.Red;
            myPane.YAxis.Title.FontSpec.FontColor = Color.Red;
            // turn off the opposite tics so the Y tics don't show up on the Y2 axis
            myPane.YAxis.MajorTic.IsOpposite = false;
            myPane.YAxis.MinorTic.IsOpposite = false;
            // Don't display the Y zero line
            myPane.YAxis.MajorGrid.IsZeroLine = true;
            // Align the Y axis labels so they are flush to the axis
            myPane.YAxis.Scale.Align = AlignP.Inside;
            // Manually set the axis range
            //myPane.YAxis.Scale.Min = -1;
            //myPane.YAxis.Scale.Max = 1;

            // Fill the axis background with a gradient
            //myPane.Chart.Fill = new Fill(Color.White, Color.LightGray, 45.0f);

            // Calculate the Axis Scale Ranges
            try
            {
                zg1.AxisChange();
            }
            catch
            {
            }
        }

        private void Graphit_Click(object sender, EventArgs e)
        {
            LogToKML();
            //graphit_clickprocess(true);
        }

        void graphit_clickprocess(bool left = true)
        {
            if (dataGridView1 == null || dataGridView1.RowCount == 0 || dataGridView1.ColumnCount == 0)
            {
                CustomMessageBox.Show(Strings.PleaseLoadValidFile, Strings.ERROR);
                return;
            }

            if (dataGridView1.CurrentCell == null)
            {
                CustomMessageBox.Show(Strings.PleaseSelectCell, Strings.ERROR);
                return;
            }

            int col = dataGridView1.CurrentCell.ColumnIndex;
            int row = dataGridView1.CurrentCell.RowIndex;
            string type = dataGridView1[typecoloum, row].Value.ToString();

            if (col == 0)
            {
                CustomMessageBox.Show("Please pick another column, Highlight the cell you wish to graph", Strings.ERROR);
                return;
            }

            if (!dflog.logformat.ContainsKey(type))
            {
                CustomMessageBox.Show(Strings.NoFMTMessage + type, Strings.ERROR);
                return;
            }

            if ((col - typecoloum - 1) < 0)
            {
                CustomMessageBox.Show(Strings.CannotGraphField, Strings.ERROR);
                return;
            }

            if (dflog.logformat[type].FieldNames.Length <= (col - typecoloum - 1))
            {
                CustomMessageBox.Show(Strings.InvalidField, Strings.ERROR);
                return;
            }

            string fieldname = dflog.logformat[type].FieldNames[col - typecoloum - 1];

            GraphItem(type, fieldname, left);
        }

        void GraphItem(string type, string fieldname, bool left = true, bool displayerror = true,
            bool isexpression = false)
        {
            DataModifer dataModifier = new DataModifer();
            string nodeName = DataModifer.GetNodeName(type, fieldname);

            foreach (var curve in zg1.GraphPane.CurveList)
            {
                // its already on the graph, abort
                if (curve.Label.Text.Equals(nodeName) ||
                    curve.Label.Text.Equals(nodeName + " R"))
                    return;
            }

            if (dataModifierHash.ContainsKey(nodeName))
            {
                dataModifier = (DataModifer)dataModifierHash[nodeName];
            }

            /*
            // ensure we tick the treeview
            foreach (TreeNode node in treeView1.Nodes)
            {
                if (node.Text == type)
                {
                    foreach (TreeNode subnode in node.Nodes)
                    {
                        if (subnode.Text == fieldname && subnode.Checked != true)
                        {
                            subnode.Checked = true;
                            break;
                        }
                    }
                }
            }
            */
            if (!isexpression)
            {
                if (!dflog.logformat.ContainsKey(type))
                {
                    if (displayerror)
                        CustomMessageBox.Show(Strings.NoFMTMessage + type + " - " + fieldname, Strings.ERROR);
                    return;
                }

                log.Info("Graphing " + type + " - " + fieldname);

                Loading.ShowLoading("Graphing " + type + " - " + fieldname, this);
                GraphItem_GetList(fieldname, type, dflog, dataModifier, left);
                //ThreadPool.QueueUserWorkItem(o => GraphItem_GetList(fieldname, type, dflog, dataModifier, left));
            }
            else
            {
                // var list1 = DFLogScript.ProcessExpression(ref dflog, ref logdata, type);
                // GraphItem_AddCurve(list1, type, fieldname, left);
            }
        }

        void GraphItem_GetList(string fieldname, string type, DFLog dflog, DataModifer dataModifier, bool left)
        {
            int col = dflog.FindMessageOffset(type, fieldname);

            // field does not exist
            if (col == -1)
                return;

            PointPairList list1 = new PointPairList();

            int error = 0;

            double a = 0; // row counter
            double b = 0;
            DateTime screenupdate = DateTime.MinValue;
            double value_prev = 0;

            foreach (var item in logdata.GetEnumeratorType(type))
            {
                b = item.lineno;

                if (screenupdate.Second != DateTime.Now.Second)
                {
                    Console.Write(b + " of " + logdata.Count + "     \r");
                    screenupdate = DateTime.Now;
                }

                if (item.msgtype == type)
                {
                    try
                    {
                        double value = double.Parse(item.items[col],
                            System.Globalization.CultureInfo.InvariantCulture);

                        // abandon realy bad data
                        if (Math.Abs(value) > 3.15e8)
                        {
                            a++;
                            continue;
                        }

                        if (dataModifier.IsValid())
                        {
                            if ((a != 0) && Math.Abs(value - value_prev) > 1e5)
                            {
                                // there is a glitch in the data, reject it by replacing it with the previous value
                                value = value_prev;
                            }
                            value_prev = value;

                            if (dataModifier.doOffsetFirst)
                            {
                                value += dataModifier.offset;
                                value *= dataModifier.scalar;
                            }
                            else
                            {
                                value *= dataModifier.scalar;
                                value += dataModifier.offset;
                            }
                        }

                        if (chk_time.Checked)
                        {
                            var e = new DataGridViewCellValueEventArgs(1, (int)b);
                            dataGridView1_CellValueNeeded(dataGridView1, e);

                            XDate time = new XDate(DateTime.Parse(e.Value.ToString()));

                            list1.Add(time, value);
                        }
                        else
                        {
                            list1.Add(b, value);
                        }
                    }
                    catch
                    {
                        error++;
                        log.Info("Bad Data : " + type + " " + col + " " + a);
                        if (error >= 500)
                        {
                            CustomMessageBox.Show("There is to much bad data - failing");
                            break;
                        }
                    }
                }

                a++;
            }

            //Invoke((Action)delegate
           // {
                GraphItem_AddCurve(list1, type, fieldname, left);
           // });
        }

        void GraphItem_AddCurve(PointPairList list1, string type, string header, bool left)
        {
            if (list1.Count < 1)
            {
                Loading.Close();
                return;
            }

            LineItem myCurve;

            myCurve = zg1.GraphPane.AddCurve(type + "." + header, list1,
                colours[zg1.GraphPane.CurveList.Count % colours.Length], SymbolType.None);

            /*

            ///doing this in treeview check event.
            leftorrightaxis(left, myCurve);

            // Make sure the Y axis is rescaled to accommodate actual data
            try
            {
                zg1.AxisChange();
            }
            catch
            {
            }
            // Zoom all
            zg1.ZoomOutAll(zg1.GraphPane);

            try
            {
                DrawModes();

                DrawErrors();

                DrawTime();
            }
            catch
            {
            }
            //zg1.Refresh();
            //zg1.Invalidate();
            //zg1.RestoreScale(zg1.GraphPane);
            */
            //Grf_SetScaleToDefault();  //zg1.RestoreScale(zg1.GraphPane);
        }

        void DrawErrors()
        {
            bool top = false;
            double a = 0;

            if (ErrorCache.Count > 0)
            {
                foreach (var item in ErrorCache)
                {
                    item.Location.Y = zg1.GraphPane.YAxis.Scale.Max;
                    zg1.GraphPane.GraphObjList.Add(item);
                }
                return;
            }

            ErrorCache.Clear();

            double b = 0;

            //ErrorCache.Add(new TextObj("", -500, 0));

            if (!dflog.logformat.ContainsKey("ERR"))
                return;

            foreach (var item in logdata.GetEnumeratorType("ERR"))
            {
                b = item.lineno;

                if (item.msgtype == "ERR")
                {
                    if (!dflog.logformat.ContainsKey("ERR"))
                        return;

                    int index = dflog.FindMessageOffset("ERR", "Subsys");
                    if (index == -1)
                    {
                        continue;
                    }

                    int index2 = dflog.FindMessageOffset("ERR", "ECode");
                    if (index2 == -1)
                    {
                        continue;
                    }

                    if (chk_time.Checked)
                    {
                        XDate date = new XDate(item.time);
                        b = date.XLDate;
                    }

                    string mode = "Err: " + ((DFLog.error_subsystem)int.Parse(item.items[index].ToString())) + "-" +
                                  item.items[index2].ToString().Trim();
                    if (top)
                    {
                        var temp = new TextObj(mode, b, zg1.GraphPane.YAxis.Scale.Max, CoordType.AxisXYScale,
                            AlignH.Left, AlignV.Top);
                        temp.FontSpec.Fill.Color = Color.Red;
                        ErrorCache.Add(temp);
                        zg1.GraphPane.GraphObjList.Add(temp);
                    }
                    else
                    {
                        var temp = new TextObj(mode, b, zg1.GraphPane.YAxis.Scale.Max, CoordType.AxisXYScale,
                            AlignH.Left, AlignV.Bottom);
                        temp.FontSpec.Fill.Color = Color.Red;
                        ErrorCache.Add(temp);
                        zg1.GraphPane.GraphObjList.Add(temp);
                    }
                    top = !top;
                }
                a++;
            }
        }

        void DrawModes()
        {
            bool top = false;
            double a = 0;

            zg1.GraphPane.GraphObjList.Clear();

            if (ModeCache.Count > 0)
            {
                foreach (var item in ModeCache)
                {
                    item.Location.Y = zg1.GraphPane.YAxis.Scale.Min;
                    zg1.GraphPane.GraphObjList.Add(item);
                }
                return;
            }

            ModeCache.Clear();
            string lastMode = "";
            foreach (var item in logdata.GetEnumeratorType("RC"))
            {
                a = item.lineno;

                if (item.msgtype == "RC")
                {

                    int index = dflog.FindMessageOffset("RC", "Ch5");
                    if (index == -1)
                    {
                        continue;
                    }

                    if (chk_time.Checked)
                    {
                        XDate date = new XDate(item.time);
                        a = date.XLDate;
                    }

                    string mode = getCh5Mode(double.Parse(item.items[index]));
                    if (lastMode != mode) { 
                        lastMode = mode;
                        if (mode != null) {
                           // if (top)
                           // {
                                var temp = new TextObj(mode, a, zg1.GraphPane.YAxis.Scale.Min, CoordType.AxisXYScale,
                                    AlignH.Left, AlignV.Top);
                                ModeCache.Add(temp);
                                zg1.GraphPane.GraphObjList.Add(temp);
                          //  }
                            /*
                            else
                            {
                                var temp = new TextObj(mode, a, zg1.GraphPane.YAxis.Scale.Min, CoordType.AxisXYScale,
                                    AlignH.Left, AlignV.Bottom);
                                ModeCache.Add(temp);
                                zg1.GraphPane.GraphObjList.Add(temp);
                            }
                            top = !top;*/
                        }
                    }
                }
                a++;
            }
        }


        public static string getCh5Mode(double ch5Mode) 
        {
            double rndCh5 = Math.Round(ch5Mode, 2);
            if (rndCh5 == -0.58 && ch5Mode<0.0f) return "Start";
            if (rndCh5 == -0.15 && ch5Mode<0.0f) return "Takeoff/Land";
            if (rndCh5 == 0.57 && ch5Mode > 0) return "Pause";
            if (rndCh5 == 0.14 && ch5Mode > 0) return "RTH";
            return null;
        }

        /// <summary>
        /// Shows the time block on to of Graph  (not used when united by time (chk_time.Checked))
        /// </summary>
        void DrawTime()
        {
            if (chk_time.Checked)
                return;
            DateTime starttime = DateTime.MinValue;
            UInt64 startdelta = 0;
            DateTime workingtime = starttime;

            DateTime lastdrawn = DateTime.MinValue;


            if (TimeCache.Count > 0)
            {
                foreach (var item in TimeCache)
                {
                    item.Location.Y = zg1.GraphPane.YAxis.Scale.Max;
                    zg1.GraphPane.GraphObjList.Add(item);
                }
                return;
            }

            double b = 0;

            foreach (var item in logdata.GetEnumeratorType("GPS"))
            {
                b = item.lineno;
                int index = dflog.FindMessageOffset("GPS", "GPSTime");

                //DateTime dt = DFLog.EpochMicro(double.Parse(item.items[index]));
                UInt64 time = UInt64.Parse(item.items[index]);
                        if (startdelta == 0)
                            startdelta = time;

                        workingtime = starttime.AddMilliseconds((double)(time - startdelta)/1000);
                        TimeSpan span = workingtime - starttime;

                        if (workingtime.Minute != lastdrawn.Minute)
                        {
                            var temp = new TextObj(span.TotalMinutes.ToString("0") + " min", b,
                                zg1.GraphPane.YAxis.Scale.Max, CoordType.AxisXYScale, AlignH.Left, AlignV.Top);
                            TimeCache.Add(temp);
                            zg1.GraphPane.GraphObjList.Add(temp);
                            lastdrawn = workingtime;
                        }
            }
        }

        class LogRouteInfo
        {
            public int firstpoint = 0;
            public int lastpoint = 0;
            public List<int> samples = new List<int>();
        }

        void DrawMap()
        {
            int rtcnt = 0;

            try
            {
                mapoverlay.Routes.Clear();

                DateTime starttime = DateTime.MinValue;
                DateTime workingtime = starttime;

                DateTime lastdrawn = DateTime.MinValue;

                List<PointLatLng> routelist = new List<PointLatLng>();
                List<int> samplelist = new List<int>();

                List<PointLatLng> routelistpos = new List<PointLatLng>();
                List<int> samplelistpos = new List<int>();

                //zg1.GraphPane.GraphObjList.Clear();

                //check if GPS data are available
                if (!dflog.logformat.ContainsKey("GPS"))
                    return;

                int latindex = dflog.FindMessageOffset("GPS", "Lat");
                if (latindex == -1)
                {
                    return;
                }

                int lngindex2 = dflog.FindMessageOffset("GPS", "Lon");
                if (lngindex2 == -1)
                {
                    return;
                }

                int statusindex3 = dflog.FindMessageOffset("GPS", "Fix");
                if (statusindex3 == -1)
                {
                    return;
                }

                int poslatindex = -1;
                int poslngindex = -1;
                int posaltindex = -1;
                // check for POS message
                if (dflog.logformat.ContainsKey("GPOS"))
                {
                    poslatindex = dflog.FindMessageOffset("GPOS", "Lat");
                    poslngindex = dflog.FindMessageOffset("GPOS", "Lon");
                    posaltindex = dflog.FindMessageOffset("GPOS", "Alt");
                }

                int i = 0;
                int firstpoint = 0;
                int firstpointpos = 0;

                foreach (var item in logdata.GetEnumeratorType(new string[] { "GPS", "GPOS", "GPS2" }))
                {
                    i = item.lineno;

                    if (item.msgtype == "GPS")
                    {
                        var ans = getPointLatLng(item);

                        if (ans.HasValue)
                        {
                            routelist.Add(ans.Value);
                            samplelist.Add(i);

                            if (routelist.Count > 1000)
                            {
                                //split the route in several small parts (due to memory errors)
                                GMapRoute route_part = new GMapRoute(routelist, "route_" + rtcnt);
                                route_part.Stroke = new Pen(Color.FromArgb(127, Color.OrangeRed), 2);

                                LogRouteInfo lri = new LogRouteInfo();
                                lri.firstpoint = firstpoint;
                                lri.lastpoint = i;
                                lri.samples.AddRange(samplelist);

                                route_part.Tag = lri;
                                route_part.IsHitTestVisible = true;
                                mapoverlay.Routes.Add(route_part);
                                rtcnt++;

                                //clear the list and set the last point as first point for the next route
                                routelist.Clear();
                                samplelist.Clear();
                                firstpoint = i;
                                samplelist.Add(firstpoint);
                                routelist.Add(ans.Value);
                            }
                        }
                    }

                    if (item.msgtype == "GPOS")
                    {
                        var ans = getPointLatLng(item);

                        if (ans.HasValue)
                        {
                            routelistpos.Add(ans.Value);
                            samplelistpos.Add(i);

                            if (routelistpos.Count > 1000)
                            {
                                //split the route in several small parts (due to memory errors)
                                GMapRoute route_part = new GMapRoute(routelistpos, "routepos_" + rtcnt);
                                route_part.Stroke = new Pen(Color.FromArgb(127, Color.Yellow), 2);

                                LogRouteInfo lri = new LogRouteInfo();
                                lri.firstpoint = firstpointpos;
                                lri.lastpoint = i;
                                lri.samples.AddRange(samplelistpos);

                                route_part.Tag = lri;
                                route_part.IsHitTestVisible = false;
                                mapoverlay.Routes.Add(route_part);
                                rtcnt++;

                                //clear the list and set the last point as first point for the next route
                                routelistpos.Clear();
                                samplelistpos.Clear();
                                firstpointpos = i;
                                samplelistpos.Add(firstpoint);
                                routelistpos.Add(ans.Value);
                            }
                        }
                    }
                    i++;
                }

                GMapRoute route = new GMapRoute(routelist, "route_" + rtcnt);
                route.Stroke = new Pen(Color.FromArgb(127, Color.Blue), 2);
                route.IsHitTestVisible = true;

                LogRouteInfo lri2 = new LogRouteInfo();
                lri2.firstpoint = firstpoint;
                lri2.lastpoint = i;
                lri2.samples.AddRange(samplelist);
                route.Tag = lri2;
                route.IsHitTestVisible = true;
                mapoverlay.Routes.Add(route);
                rtcnt++;
                gMapControl1.ZoomAndCenterRoutes(mapoverlay.Id);
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
            if (rtcnt > 0)
                gMapControl1.RoutesEnabled = true;
        }

        PointLatLng? getPointLatLng(DFLog.DFItem item)
        {
            if (item.msgtype == "GPS")
            {
                if (!dflog.logformat.ContainsKey("GPS"))
                    return null;

                int index = dflog.FindMessageOffset("GPS", "Lat");
                if (index == -1)
                {
                    return null;
                }

                int index2 = dflog.FindMessageOffset("GPS", "Lon");
                if (index2 == -1)
                {
                    return null;
                }

                int index3 = dflog.FindMessageOffset("GPS", "Fix");
                if (index3 == -1)
                {
                    return null;
                }

                try
                {
                    if (double.Parse(item.items[index3].ToString(), System.Globalization.CultureInfo.InvariantCulture) <
                        3)
                    {
                        return null;
                    }

                    string lat = item.items[index].ToString();
                    string lng = item.items[index2].ToString();

                    PointLatLng pnt = new PointLatLng() { };
                    pnt.Lat = double.Parse(lat, System.Globalization.CultureInfo.InvariantCulture);
                    pnt.Lng = double.Parse(lng, System.Globalization.CultureInfo.InvariantCulture);

                    return pnt;
                }
                catch
                {
                }
            }

            if (item.msgtype == "GPOS")
            {
                if (!dflog.logformat.ContainsKey("GPOS"))
                    return null;

                int index = dflog.FindMessageOffset("GPOS", "Lat");
                if (index == -1)
                {
                    return null;
                }

                int index2 = dflog.FindMessageOffset("GPOS", "Lon");
                if (index2 == -1)
                {
                    return null;
                }

                try
                {
                    string lat = item.items[index].ToString();
                    string lng = item.items[index2].ToString();

                    PointLatLng pnt = new PointLatLng() { };
                    pnt.Lat = double.Parse(lat, System.Globalization.CultureInfo.InvariantCulture);
                    pnt.Lng = double.Parse(lng, System.Globalization.CultureInfo.InvariantCulture);

                    if (Math.Abs(pnt.Lat) > 90 || Math.Abs(pnt.Lng) > 180)
                        return null;

                    return pnt;
                }
                catch
                {
                }
            }

            return null;
        }

        int FindInArray(string[] array, string find)
        {
            int a = 0;
            foreach (string item in array)
            {
                if (item == find)
                {
                    return a;
                }
                a++;
            }
            return -1;
        }

        private void leftorrightaxis(bool left, CurveItem myCurve)
        {
            if (!left)
            {
                myCurve.Label.Text += " R";
                myCurve.IsY2Axis = true;
                myCurve.YAxisIndex = 0;
                zg1.GraphPane.Y2Axis.IsVisible = true;
            }
            else if (left)
            {
                myCurve.IsY2Axis = false;
            }
        }

        private void BUT_cleargraph_Click(object sender, EventArgs e)
        {
            clearGraph();
        }

        private void cleargraph_Click(object sender, EventArgs e)
        {
            clearGraph();
        }

        private void clearGraph()
        {
            zg1.GraphPane.CurveList.Clear();
            zg1.GraphPane.GraphObjList.Clear();
            zg1.Invalidate();
            UntickTreeView();
        }


        private void BUT_loadlog_Click(object sender, EventArgs e)
        {
            // clear existing lists
            zg1.GraphPane.CurveList.Clear();
            // reset logname
            logfilename = "";
            // reload
            LogBrowse_Load();
        }

        private void dataGridView1_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            Point mp = Control.MousePosition;

            List<string> options = new List<string>();

            int b = 0;

            foreach (var item2 in logdata)
            {
                b++;
                var item = dflog.GetDFItemFromLine(item2, b);

                if (item.msgtype == null)
                    continue;

                string celldata = item.msgtype.Trim();
                if (!options.Contains(celldata))
                {
                    options.Add(celldata);
                }
            }

            OptionForm opt = new OptionForm();

            opt.StartPosition = FormStartPosition.Manual;
            opt.Location = mp;

            opt.Combobox.DataSource = options;
            opt.Button1.Text = "Filter";
            opt.Button2.Text = "Cancel";

            opt.ShowDialog(this);

            if (opt.SelectedItem != "")
            {
                logdatafilter.Clear();

                int a = 0;
                b = 0;

                foreach (var item2 in logdata)
                {
                    b++;
                    var item = dflog.GetDFItemFromLine(item2, b);

                    if (item.msgtype == opt.SelectedItem)
                    {
                        logdatafilter.Add(a, item);
                        a++;
                    }
                }

                dataGridView1.Rows.Clear();
                dataGridView1.RowCount = logdatafilter.Count;

            }
            else
            {
                logdatafilter.Clear();
                dataGridView1.Rows.Clear();
                dataGridView1.RowCount = logdata.Count;
            }

            /*
            dataGridView1.SuspendLayout();
            
            foreach (DataGridViewRow datarow in dataGridView1.Rows)
            {
                string celldata = datarow.Cells[0].Value.ToString().Trim();
                if (celldata == opt.SelectedItem || opt.SelectedItem == "")
                    datarow.Visible = true;
                else
                {
                    try
                    {
                        datarow.Visible = false;
                    }
                    catch { }
                }
            }

            dataGridView1.ResumeLayout();
             * */
            dataGridView1.Invalidate();
        }

        void BUT_go_Click(object sender, EventArgs e)
        {
            Button but = sender as Button;
        }

        /// <summary>
        /// Update row number display for those only in view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView1_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
        }

        private void BUT_Graphit_R_Click(object sender, EventArgs e)
        {
            graphit_clickprocess(false);
        }

        private void zg1_ZoomEvent(ZedGraphControl sender, ZoomState oldState, ZoomState newState)
        {
            try
            {
                DrawModes();
                //DrawErrors();
                DrawTime();
            }
            catch
            {
            }
        }

        private void CHK_map_CheckedChanged(object sender, EventArgs e)
        {
            splitContainerGrf.Panel2Collapsed = !splitContainerGrf.Panel2Collapsed;

   
                log.Info("Get map");

                // DrawMap();

                log.Info("map done");
            
        }

        private void BUT_removeitem_Click(object sender, EventArgs e)
        {
            Point mp = Control.MousePosition;

            OptionForm opt = new OptionForm();

            opt.StartPosition = FormStartPosition.Manual;
            opt.Location = mp;

            List<string> list = new List<string>();

            zg1.GraphPane.CurveList.ForEach(x => list.Add(x.Label.Text));

            opt.Combobox.DataSource = list.ToArray();
            opt.Button1.DialogResult = System.Windows.Forms.DialogResult.OK;
            opt.Button1.Text = "Remove";
            opt.Button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            opt.Button2.Text = "Cancel";

            if (opt.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
            {
                if (opt.SelectedItem != "")
                {
                    foreach (var item in zg1.GraphPane.CurveList)
                    {
                        if (item.Label.Text == opt.SelectedItem)
                        {
                            zg1.GraphPane.CurveList.Remove(item);
                            break;
                        }
                    }
                }
            }

            zg1.Invalidate();
        }

        private void treeView1_DoubleClick(object sender, EventArgs e)
        {
            // apply a slope and offset to a selected child
            if (treeView1.SelectedNode == null || treeView1.SelectedNode.Parent == null)
            {
                // only apply scalers to children
                return;
            }

            string dataModifer_str = "";
            string nodeName = DataModifer.GetNodeName(treeView1.SelectedNode.Parent.Text, treeView1.SelectedNode.Text);

            if (dataModifierHash.ContainsKey(nodeName))
            {
                DataModifer initialDataModifier = (DataModifer)dataModifierHash[nodeName];
                if (initialDataModifier.IsValid())
                    dataModifer_str = initialDataModifier.commandString;
            }

            string title = "Apply scaler and offset to " + nodeName;
            string instructions =
                "Enter modifer then value, they are applied in the order you provide. Modifiers are x + - /\n";
            instructions += "Example: Convert cm to to m with an offset of 50: '/100 +50' or 'x0.01 +50' or '*0.01,+50'";
            InputBox.Show(title, instructions, ref dataModifer_str);

            // if it's already there, remove it.
            dataModifierHash.Remove(nodeName);

            DataModifer dataModifer = new DataModifer(dataModifer_str);
            if (dataModifer.IsValid())
            {
                dataModifierHash.Add(nodeName, dataModifer);
            }
        }

        private void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {

        }

        private void CMB_preselect_SelectedIndexChanged(object sender, EventArgs e)
        {
            displaylist selectlist = (displaylist)CMB_preselect.SelectedValue;

            if (selectlist == null || selectlist.items == null)
                return;

            clearGraph();

            foreach (var item in selectlist.items)
            {
                try
                {
                    if (!string.IsNullOrEmpty(item.expression))
                    {
                        GraphItem(item.expression, "", item.left, false, true);
                    }
                    else
                    {
                        GraphItem(item.type, item.field, item.left, false);
                    }
                }
                catch
                {
                }

            }
            zg1.RestoreScale(zg1.GraphPane);
            zg1.Refresh();
        }

        private void treeView1_DrawNode(object sender, DrawTreeNodeEventArgs e)
        {
            var area = e.Node.Bounds;

            if (e.Node.Parent == null)
            {
                area.X -= 17;
                area.Width += 17;
            }

            using (SolidBrush brush = new SolidBrush(treeView1.BackColor))
            {
                e.Graphics.FillRectangle(brush, area);
            }


            TextRenderer.DrawText(e.Graphics, e.Node.Text, treeView1.Font, e.Node.Bounds, treeView1.ForeColor,
                treeView1.BackColor);

            if ((e.State & TreeNodeStates.Focused) == TreeNodeStates.Focused)
            {
                ControlPaint.DrawFocusRectangle(e.Graphics, e.Node.Bounds, treeView1.ForeColor, treeView1.BackColor);
            }
        }

        private void LogBrowse_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (logdata != null)
                logdata.Clear();
            logdata = null;
            m_dtCSV = null;
            dataGridView1.DataSource = null;
            mapoverlay = null;
            GC.Collect();
        }

        private void dataGridView1_CellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
        {
            try
            {
                var item2 = logdata[e.RowIndex];

                var item = dflog.GetDFItemFromLine(item2, e.RowIndex);

                if (logdatafilter.Count > 0)
                {
                    item = (DFLog.DFItem)logdatafilter[e.RowIndex];
                }

                if (e.ColumnIndex == 0)
                {
                    e.Value = item.lineno;
                }
                else if (e.ColumnIndex == 1)
                {
                    e.Value = item.time.ToString("yyyy-MM-dd HH:mm:ss.fff");
                }
                else if (item.items != null && e.ColumnIndex < item.items.Length + 2)
                {
                    e.Value = item.items[e.ColumnIndex - 2];
                }
                else
                {
                    e.Value = null;
                }
            }
            catch
            {
            }
        }


        private void zg1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ZedGraph.ZedGraphControl ctrl = (ZedGraph.ZedGraphControl)sender;
            PointF ptClick = new PointF(e.X, e.Y);
            double x, y;

            ctrl.GraphPane.ReverseTransform(ptClick, out x, out y);

            object nearestObject;
            int index;
            int row = 0;
            ctrl.GraphPane.FindNearestObject(ptClick, this.CreateGraphics(), out nearestObject, out index);
            if (nearestObject != null && nearestObject.GetType() == typeof(LineItem))
            {
                LineItem lnItem = (LineItem)nearestObject;
                row = (int)lnItem[index].X;
                //zg1.Invalidate();
            }
            GoToSample(row, true, false, true);
        }

        private void scrollGrid(DataGridView dataGridView, int index)
        {
            int halfWay = (dataGridView.DisplayedRowCount(false) / 2);

            if ((index < 0) && (dataGridView.SelectedRows.Count > 0))
            {
                index = dataGridView.SelectedRows[0].Index;
            }

            if (dataGridView.FirstDisplayedScrollingRowIndex + halfWay > index ||
                (dataGridView.FirstDisplayedScrollingRowIndex + dataGridView.DisplayedRowCount(false) - halfWay) <=
                index)
            {
                int targetRow = index;

                targetRow = Math.Max(targetRow - halfWay, 0);
                try
                {
                    dataGridView.FirstDisplayedScrollingRowIndex = targetRow;
                }
                catch
                {
                    //soft fail
                }
            }
        }





        bool GetHomeFromRow(int lineNumber, out PointLatLng pt)
        {
            string HOME = "HOME";
            bool ret = false;
            int index_lat = -1;
            int index_lng = -1;
            pt = new PointLatLng();

            if (lineNumber >= logdata.Count)
                return ret;

            if (!dflog.logformat.ContainsKey(HOME))
                return ret;

            int roffset = 0;
            bool found = false;
            //only search past logs
            for (int i = 0; lineNumber - i > 0 && !found; i++)
            {
                string searching = logdata[lineNumber - i];
                if (searching.StartsWith(HOME))
                {
                    roffset = i;
                    found = true;
                }
            }
            lineNumber -= roffset;
            ret = found;

            if (ret)
            {
                string gpsline = logdata[lineNumber];
                var item = dflog.GetDFItemFromLine(gpsline, lineNumber);
                if (gpsline.StartsWith(HOME))
                {
                    index_lat = dflog.FindMessageOffset("HOME", "Lat");
                    index_lng = dflog.FindMessageOffset("HOME", "Lon");

                    if (index_lat < 0 || index_lng < 0)
                        ret = false;
                }

                if (ret)
                {
                    string lat = item.items[index_lat];
                    string lng = item.items[index_lng];
                    pt.Lat = double.Parse(lat, System.Globalization.CultureInfo.InvariantCulture);
                    pt.Lng = double.Parse(lng, System.Globalization.CultureInfo.InvariantCulture);
                }
            }
            return ret;
        }



        bool GetTimeFromRow(int lineNumber, out int millis)
        {
            bool ret = false;
            millis = 0;

            if (!dflog.logformat.ContainsKey("IMU"))
                return ret;

            int index = dflog.FindMessageOffset("IMU", "TimeMS");
            if (index < 0)
                return ret;

            const int maxSearch = 100;


            for (int i = 0; i < maxSearch; i++)
            {
                for (int s = -1; s < 2; s = s + 2)
                {
                    int r = lineNumber + s * i;
                    if ((r >= 0) && (r < m_dtCSV.Rows.Count))
                    {
                        DataRow datarow = m_dtCSV.Rows[r];

                        if (datarow[1].ToString() == "IMU")
                        {
                            try
                            {
                                string mil = datarow[index + 2].ToString();
                                millis = int.Parse(mil);
                                ret = true;
                                break;
                            }
                            catch
                            {
                            }
                        }
                    }
                }
                if (ret)
                    break;
            }

            return ret;
        }

        bool GetGPSFromRow(int lineNumber, out PointLatLng pt)
        {
            bool ret = false;
            int index_lat = -1;
            int index_lng = -1;
            pt = new PointLatLng();

            if (lineNumber >= logdata.Count)
                return ret;

            if (!dflog.logformat.ContainsKey("GPS") && !dflog.logformat.ContainsKey("GPOS"))
                return ret;

            const int maxSearch = 1000;
            int offset = maxSearch;
            int roffset = -maxSearch;
            bool found = false;

            for (int i = 0; i < maxSearch && lineNumber + i < logdata.Count && !found; i++)
            {
                string searching = logdata[lineNumber + i];
                if (searching.StartsWith("GPS") || searching.StartsWith("GPOS"))
                {
                    offset = i;
                    found = true;
                }
            }

            found = false;
            for (int i = 0; i < maxSearch && lineNumber - i >= 0 && !found; i++)
            {
                string searching = logdata[lineNumber - i];
                if (searching.StartsWith("GPS") || searching.StartsWith("GPOS"))
                {
                    roffset = i;
                    found = true;
                }
            }

            if (offset < roffset)
            {
                lineNumber += offset;
                ret = true;
            }
            else if (roffset < maxSearch)
            {
                lineNumber -= roffset;
                ret = true;
            }

            if (ret == true)
            {
                string gpsline = logdata[lineNumber];
                var item = dflog.GetDFItemFromLine(gpsline, lineNumber);
                if (gpsline.StartsWith("GPS"))
                {
                    index_lat = dflog.FindMessageOffset("GPS", "Lat");
                    index_lng = dflog.FindMessageOffset("GPS", "Lon");
                    int index_status = dflog.FindMessageOffset("GPS", "Fix");

                    if (index_status < 0)
                        ret = false;

                    int status = int.Parse(item.items[index_status], System.Globalization.CultureInfo.InvariantCulture);
                    if (status < 3)
                        ret = false;
                }
                else if (gpsline.StartsWith("GPOS"))
                {
                    index_lat = dflog.FindMessageOffset("GPOS", "Lat");
                    index_lng = dflog.FindMessageOffset("GPOS", "Lon");
                }

                if (index_lat < 0 || index_lng < 0)
                    ret = false;

                if (ret)
                {
                    string lat = item.items[index_lat];
                    string lng = item.items[index_lng];

                    pt.Lat = double.Parse(lat, System.Globalization.CultureInfo.InvariantCulture);
                    pt.Lng = double.Parse(lng, System.Globalization.CultureInfo.InvariantCulture);
                }
            }

            return ret;
        }


        private void myGMAP1_OnRouteClick(GMapRoute item, MouseEventArgs e)
        {
            if ((item.Name != null) && (item.Name.StartsWith("route_")))
            {
                LogRouteInfo lri = item.Tag as LogRouteInfo;
                if (lri != null)
                {
                    //cerco il punto più vicino
                    MissionPlanner.Utilities.PointLatLngAlt pt2 =
                        new MissionPlanner.Utilities.PointLatLngAlt(gMapControl1.FromLocalToLatLng(e.X, e.Y));
                    double dBest = double.MaxValue;
                    int nBest = 0;
                    for (int i = 0; i < item.LocalPoints.Count; i++)
                    {
                        PointLatLng pt = item.Points[i];
                        double d =
                            Math.Sqrt((pt.Lat - pt2.Lat) * (pt.Lat - pt2.Lat) + (pt.Lng - pt2.Lng) * (pt.Lng - pt2.Lng));
                        if (d < dBest)
                        {
                            dBest = d;
                            nBest = i;
                        }
                    }
                    double perc = (double)nBest / (double)item.LocalPoints.Count;
                    int SampleID = (int)(lri.firstpoint + (lri.lastpoint - lri.firstpoint) * perc);

                    if ((lri.samples.Count > 0) && (nBest < lri.samples.Count))
                        SampleID = lri.samples[nBest];

                    GoToSample(SampleID, false, true, true);


                    //debugging route click
                    //GMapMarker pos2 = new GMarkerGoogle(pt2, GMarkerGoogleType.orange_dot);
                    //markeroverlay.Markers.Add(pos2);
                }
            }
        }

        private void GoToSample(int SampleID, bool movemap, bool movegraph, bool movegrid)
        {
            markeroverlay.Markers.Clear();

            PointLatLng ptCurrPos;
            PointLatLng ptHomePos;

            if (GetHomeFromRow(SampleID, out ptHomePos))
            {
                MissionPlanner.Utilities.PointLatLngAlt ptH = new MissionPlanner.Utilities.PointLatLngAlt(ptHomePos);
                GMapMarker posH = new GMarkerGoogle(ptH, GMarkerGoogleType.lightblue_dot);
                markeroverlay.Markers.Add(posH);
            }
            if (GetGPSFromRow(SampleID, out ptCurrPos))
            {

                Bitmap xspico = new Bitmap(Path.GetDirectoryName(Application.ExecutablePath) + Path.DirectorySeparatorChar +"xsp-topgoogle.png");
                MissionPlanner.Utilities.PointLatLngAlt ptC = new MissionPlanner.Utilities.PointLatLngAlt(ptCurrPos);
                GMapMarker posC = new GMarkerGoogle(ptC, xspico);
                markeroverlay.Markers.Add(posC);
                if (movemap)
                {
                    gMapControl1.Position = ptCurrPos;
                }
            }






            //move the graph "cursor"
            if (m_cursorLine != null)
            {
                zg1.GraphPane.GraphObjList.Remove(m_cursorLine);
            }
            m_cursorLine = new LineObj(Color.Black, SampleID, 0, SampleID, 1);

            m_cursorLine.Location.CoordinateFrame = CoordType.XScaleYChartFraction; // This do the trick !
            m_cursorLine.IsClippedToChartRect = true;
            m_cursorLine.Line.Style = System.Drawing.Drawing2D.DashStyle.Dash;
            m_cursorLine.Line.Width = 2f;
            m_cursorLine.Line.Color = Color.OrangeRed;
            m_cursorLine.ZOrder = ZOrder.E_BehindCurves;
            zg1.GraphPane.GraphObjList.Add(m_cursorLine);


            if (movegraph)
            {
                double delta = zg1.GraphPane.XAxis.Scale.Max - zg1.GraphPane.XAxis.Scale.Min;
                zg1.GraphPane.XAxis.Scale.Min = SampleID - delta / 2;
                zg1.GraphPane.XAxis.Scale.Max = SampleID + delta / 2;
                zg1.AxisChange();
            }
            zg1.Invalidate();


            if (movegrid)
            {
                try
                {
                    int indx = SampleID;
                    if (dataGridView1.Rows.Count != logdata.Count)
                    {
                        //find closest row incase of filter
                        DataGridViewRow row = FindRow(dataGridView1, SampleID);
                        indx = (int)row.Index;
                    }

                    scrollGrid(dataGridView1, indx);
                    dataGridView1.CurrentCell = dataGridView1.Rows[indx].Cells[1];

                    dataGridView1.ClearSelection();
                    dataGridView1.Rows[indx].Selected = true;
                    dataGridView1.Rows[indx].Cells[1].Selected = true;
                }
                catch
                {
                }
            }
        }
        public static DataGridViewRow FindRow(DataGridView dgv, int searchID)
        {
            DataGridViewRow row = dgv.Rows
                .Cast<DataGridViewRow>()
                .Where(r => ((int)r.Cells[0].Value <= searchID))
                .Last();

            return row;
        }



        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            //int x = e.RowIndex;
            int x = (int)dataGridView1.Rows[e.RowIndex].Cells[0].Value;
            if ((x >= 0) && (e.RowIndex < dataGridView1.Rows.Count))
            {
                GoToSample(x, true, true, false);
            }
        }

        private void chk_time_CheckedChanged(object sender, EventArgs e)
        {
            ModeCache.Clear();
            ErrorCache.Clear();
            TimeCache.Clear();

            if (chk_time.Checked)
            {
                zg1.GraphPane.XAxis.Title.Text = "Time (sec)";

                zg1.GraphPane.XAxis.Type = AxisType.Date;
                zg1.GraphPane.XAxis.Scale.Format = "HH:mm:ss.fff";
                zg1.GraphPane.XAxis.Scale.MajorUnit = DateUnit.Minute;
                zg1.GraphPane.XAxis.Scale.MinorUnit = DateUnit.Second;
            }
            else
            {
                // Set the titles and axis labels
                zg1.GraphPane.XAxis.Type = AxisType.Linear;
                zg1.GraphPane.XAxis.Scale.Format = "f0";
                zg1.GraphPane.Title.Text = "Value Graph";
                zg1.GraphPane.XAxis.Title.Text = "Line Number";
                zg1.GraphPane.YAxis.Title.Text = "Output";
            }
        }



        private void LogToKML()
        {
            LogOutput lo = new LogOutput();
            foreach (var item in logdata)
            {
                string gpsline = item.ToString();
                lo.processLine((string) item);
            }
            lo.writeKML(logfilename+".kml");
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // clear existing lists
            zg1.GraphPane.CurveList.Clear();
            // reset logname
            logfilename = "";
            // reload
            LogBrowse_Load();

        }

        private void splitContainer2_Panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void openToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            // clear existing lists
            zg1.GraphPane.CurveList.Clear();
            // reset logname
            logfilename = "";
            LogBrowse_Load();
            
        }

        private void LogBrowse_Load(object sender, EventArgs e)
        {

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void statusStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveAs();
        }

        private void saveAs() { 
            saveFileDialog1.Filter = "Google Earth File (*.kmz)|*.kmz|GPS Exchange Format (*.gpx)|*.gpx|Log File (*.log)|*.log";
            saveFileDialog1.FilterIndex = 1;
            saveFileDialog1.RestoreDirectory = true;
   
            saveFileDialog1.FileName = logfilename.ToLower().Replace(".log", ".kmz").Replace(".bin", ".kmz");

                    if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                    {

                        string ext = saveFileDialog1.FileName.ToUpper().Substring(saveFileDialog1.FileName.Length - 3);
                // load first file
                this.Cursor = Cursors.WaitCursor;
                        this.progressBar1.Step = 1;
                        this.progressBar1.Value = 0;
                        this.progressBar1.Maximum = 100;
                        this.lblProgMsg.Text = "saving file...";
                        this.lblProgMsg.Refresh();

                if (ext == "LOG")
                {

                    BinaryLog.ConvertBin(this.logfilename, saveFileDialog1.FileName, false);

                }
                else if (ext == "KMZ" || ext == "GPX") {
                    LogOutput lo = new LogOutput();

                    int b = 0;
                    double per = 0;
                    int lastper = 0;
                    double cnt = (double)logdata.Count;
                    foreach (var item in logdata)
                    {
                        b++;
                        per = (b / cnt) * 100.0f;
                        if ((int)per != lastper)
                        {
                            lastper = (int)per;
                            progressBar1.Value = lastper;
                        }

                        string gpsline = item.ToString();
                        lo.processLine((string)item);
                    }
                    if (ext == "KMZ")
                        lo.writeKML(saveFileDialog1.FileName + ".kml");
                    else
                    {
                        lo.writeGPX(saveFileDialog1.FileName);
                    }

                }
                        this.lblProgMsg.Text = "";
                        this.progressBar1.Value = 0;
                        this.Cursor = Cursors.Default;
                        this.lblProgMsg.Refresh();
                        

            }

    }

        private void showMapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showMapToolStripMenuItem.Checked = !showMapToolStripMenuItem.Checked;
            if (!showMapToolStripMenuItem.Checked)
                   showGraphToolStripMenuItem.Checked = true;
            splitContainerGrf.Panel2Collapsed = !showMapToolStripMenuItem.Checked;



        }

        private void showGraphToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showGraphToolStripMenuItem.Checked = !showGraphToolStripMenuItem.Checked;
            if (!showGraphToolStripMenuItem.Checked)
                showMapToolStripMenuItem.Checked = true;
            splitContainerGrf.Panel1Collapsed = !showGraphToolStripMenuItem.Checked;
        }

        private void satToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.gMapControl1.MapProvider = GMap.NET.MapProviders.GoogleSatelliteMapProvider.Instance;
            hybridToolStripMenuItem.Checked = false;
            roadToolStripMenuItem.Checked = false;
            satToolStripMenuItem.Checked = true;
            gMapControl1.Refresh();
        }

        private void roadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.gMapControl1.MapProvider = GMap.NET.MapProviders.GoogleMapProvider.Instance;
            hybridToolStripMenuItem.Checked = false;
            roadToolStripMenuItem.Checked = true;
            satToolStripMenuItem.Checked = false;
            gMapControl1.Refresh();
        }

        private void hybridToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
            this.gMapControl1.MapProvider = GMap.NET.MapProviders.GoogleTerrainMapProvider.Instance;
            gMapControl1.Refresh();
            hybridToolStripMenuItem.Checked = true;
            roadToolStripMenuItem.Checked = false;
            satToolStripMenuItem.Checked = false;

        }

        private void viewToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void exitToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            this.Dispose();
            this.Close();
        }

        private void zg1_ContextMenuBuilder(ZedGraphControl sender, ContextMenuStrip menuStrip, Point mousePt, ZedGraphControl.ContextMenuObjectState objState)
        {
            ToolStripMenuItem tsi = new ToolStripMenuItem();
            tsi.Name = "clear_grf";
            tsi.Tag = "clear_grf";
            tsi.Text = "Clear Graph";
            tsi.Click += new System.EventHandler( this.cleargraph_Click);
            // Add the menu item to the menu
            menuStrip.Items.Add(tsi);
            foreach (ToolStripMenuItem i in menuStrip.Items)
            {
                /*
                if ((string)i.Tag == "set_default")
                {
                    // remove the menu item
                    menuStrip.Items.Remove(i);
                    // or, just disable the item with this
                    //item.Enabled = false; 

                    break;
                }*/
            }
        }

        private void treeView1_AfterCheck(object sender, TreeViewEventArgs e)
        {

            if (e.Node.Checked)
                {
                    GraphItem(e.Node.Parent.Text, e.Node.Text, true);
                }
                else
                {
                    List<CurveItem> removeitems = new List<CurveItem>();

                    foreach (var item in zg1.GraphPane.CurveList)
                    {
                        if (item.Label.Text.StartsWith(e.Node.Parent.Text + "." + e.Node.Text ) )
                        {
                            removeitems.Add(item);
                            //break;
                        }
                    }

                    foreach (var item in removeitems)
                        zg1.GraphPane.CurveList.Remove(item);
                }
            //only if non-threaded
            //            Control.Refresh - does an Control.Invalidate followed by Control.Update.
            //Control.Invalidate - invalidates a specific region of the Control (defaults
            //to
            //entire client area) and causes a paint message to be sent to the control.

            //Control.Update - causes the Paint event to occur immediately(Windows will
            //normally
            //wait until there are no other messages for the window to process, before
            //raising the Paint event).

            //The paint event of course is where all the drawing of your form occurs.Note
            //there is only one pending Paint event, if you call Invalidate 3 times, you
            //will still only receive one Paint event.

            //Grf_SetScaleToDefault();
            zg1.RestoreScale(zg1.GraphPane);
            zg1.Refresh();
            //  //zg1.RestoreScale(zg1.GraphPane);


        }

        public void setProgressVal(int val)
        {
            progressBar1.Value = val;
        }

        void Grf_SetScaleToDefault()
        {
            zg1.IsEnableHPan = false;
            zg1.IsEnableHZoom = false;

            for (int i = zg1.GraphPane.ZoomStack.Count * 2; i >= 0; i--)
            {
                zg1.ZoomOut(zg1.GraphPane);
            }

            zg1.GraphPane.XAxis.Scale.Min = 0;
            zg1.GraphPane.YAxis.Scale.Min = 0;

            zg1.IsEnableHPan = true;
            zg1.IsEnableHZoom = true;

        }

    }
}