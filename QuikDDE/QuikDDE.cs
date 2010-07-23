namespace QuikDDE
{
    using System;
    using System.IO;
    using System.Text;
    using NDde.Server;
    using QuikDDE;

    public class DdeData : EventArgs
    {
        private xlTable _xlData;
        private string _topicname;
        private string _item;
        public DdeData(byte[] data, string topicname, string itemname) : base()
        {
            this._xlData = new xlTable(data);
            this._item = itemname;
            this._topicname = topicname;
        }
        public xlTable DataTable
        {
            get
            {
                return this._xlData;
            }
        }
        public string TopicName
        {
            get
            {
                string str;
                str = !(!(this._topicname[0] == '[')) ? this._topicname.Substring(1, this._topicname.LastIndexOf(']') - 1) : this._topicname;
                return str;
            }
        }
        public string ItemName
        {
            get
            {
                return this._item;
            }
        }
    }
    public class ddesrv : DdeServer
    {
        public ddesrv(string service) : base(service)
        {
        }
        public override void Register()
        {
            base.Register();
        }
        public override void Unregister()
        {
            base.Unregister();
        }
        protected override DdeServer.PokeResult OnPoke(DdeConversation conversation, string item, byte[] data, int format)
        {
            StreamWriter streamWriter = new StreamWriter("ddedatalength.txt", true);
            streamWriter.WriteLine(string.Concat("Data length: ", data.Length.ToString()));
            streamWriter.Close();
            FileStream fileStream = new FileStream("ddedata.txt", FileMode.Append);
            fileStream.Write(data, 0, data.Length);
            fileStream.Dispose();
            Data(new DdeData(data, conversation.Topic, item));
            return DdeServer.PokeResult.Processed;
        }
        protected virtual void Data(DdeData e)
        {
            if (!(OnPokeData == null))
            {
                OnPokeData(this, e);
            }
        }
        public delegate void DdeEventHandler(object sender, DdeData e);
        public event ddesrv.DdeEventHandler OnPokeData;
    }
    public class xlTable
    {
        private int _rows;
        private int _cols;
        private string[,] _table;
        public xlTable(byte[] data) : base()
        {
            Update(data, 0);
        }
        public xlTable(byte[] data, int StartIndex) : base()
        {
            Update(data, StartIndex);
        }
        private void Update(byte[] data, int _StartIndx)
        {
            int i;
            int i1;
            int i2;
            int i3;
            int i6;
            bool bl;
            i = _StartIndx;
            i1 = 0;
            i2 = 0;
            i3 = 0;
            i6 = 0;
            while ((i < data.Length))
            {
                switch (data[i])
                {
                    case 0:
                        i++;
                        break;

                    case 1:
                        i3 = data[i + 2];
                        i6 = i + 4;
                        while ((i6 < (i3 + i6)))
                        {
                            this._table[i1, i2] = BitConverter.ToDouble(data, i6).ToString();
                            i2++;
                            bl = !(i2 == this._cols);
                            if (!bl)
                            {
                                i1++;
                                i2 = 0;
                            }
                            i6 += 8;
                        }
                        i = i6 + i3;
                        break;

                    case 2:
                        i3 = data[i + 2];
                        i6 = i + 4;
                        while ((i6 < (i6 + i3)))
                        {
                            this._table[i1, i2] = Encoding.Default.GetString(data, i6 + 1, data[i6]);
                            i2++;
                            bl = !(i2 == this._cols);
                            if (!bl)
                            {
                                i1++;
                                i2 = 0;
                            }
                            i6 += data[i6];
                            i6++;
                        }
                        i = i6 + i3;
                        break;

                    case 16:
                        i3 = data[2];
                        this._rows = data[4];
                        this._cols = data[6];
                        this._table = new string[this._rows, this._cols];
                        i = 8;
                        break;

                }
            }
        }
        private enum DataType
        {
            Empty = 0,
            Table = 16,
            Float = 1,
            String = 2,
            Bool = 3,
            Error = 4,
            Blank = 5,
            Int = 6,
            Skip = 7,
        }
        public int RowsCount
        {
            get
            {
                return this._rows;
            }
        }
        public int ColsCount
        {
            get
            {
                return this._cols;
            }
        }
        public string this[int RowID, int ColID]
        {
            get
            {
                string str;
                str = !((ColID >= this._cols ? false : RowID < this._rows)) ? null : this._table[RowID, ColID];
                return str;
            }
        }
    }
}