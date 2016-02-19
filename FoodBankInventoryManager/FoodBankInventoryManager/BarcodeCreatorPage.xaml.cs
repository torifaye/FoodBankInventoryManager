using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FoodBankInventoryManager
{
    /// <summary>
    /// Interaction logic for BarcodeCreatorPage.xaml
    /// </summary>
    public partial class BarcodeCreatorPage : Page
    {
        private Barcode b;
        private Bitmap dImg;
        private List<Bitmap> barcodes;
        private List<string> barcodeValues;
        private string barcodeData = "";

        public BarcodeCreatorPage()
        {
            b = new Barcode();
            barcodes = new List<Bitmap>();
            barcodeValues = new List<string>();
            InitializeComponent();
        }

        private void btnGenerateBarcode_Click(object sender, RoutedEventArgs e)
        {
            barcodeData = txtBarcodedata.Text;

            int W = 200;
            int H = 100;
            b.Alignment = AlignmentPositions.CENTER;
            TYPE type = TYPE.CODE39;
            b.IncludeLabel = true;
            b.RotateFlipType = RotateFlipType.RotateNoneFlipNone;
            b.LabelPosition = LabelPositions.BOTTOMCENTER;

            string tempbarcode = txtBarcodedata.Text.Trim().ToUpper();

            while (tempbarcode.Length < 13)
            {
                tempbarcode += " ";
            }

            try
            {

                dImg = (Bitmap)b.Encode(type, tempbarcode, System.Drawing.Color.Black, System.Drawing.Color.White, W, H);
                MemoryStream ms = new MemoryStream();
                dImg.Save(ms, ImageFormat.Jpeg);
                BitmapImage bImg = new BitmapImage();
                bImg.BeginInit();
                bImg.StreamSource = new MemoryStream(ms.ToArray());
                bImg.EndInit();
                //img is an Image control.
                imgBarcode.Source = bImg;

            }
            catch (Exception)
            {
                MessageBox.Show("An error has occured, please enter a barcode string [1-13] characters.");
                txtBarcodedata.Text = "";

            }
        }
        private void btnPrintBarcode_Click(object sender, RoutedEventArgs e)
        {
            PrintDocument printDocument1 = new PrintDocument();
            printDocument1.PrintPage += new PrintPageEventHandler(printDocument1_PrintPage);
            printDocument1.Print();
        }

        private void printDocument1_PrintPage(object sender, PrintPageEventArgs e)
        {
            try
            {
                if (barcodes.Count > 0)
                {
                    int c = 0;
                    int r = 0;

                    for (int i = 0; i < barcodes.Count; i++)
                    {

                        if (r < 10)
                        {
                            e.Graphics.DrawImage(barcodes[i], c, (barcodes[i].Height * r));
                        }
                        else
                        {
                            c += 250;
                            r = 0;
                            e.Graphics.DrawImage(barcodes[i], c, 0);
                        }

                        r++;
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show("No Barcode is currently generated, please fill-out the barcode data box and click \"Generate Barcode \"", "Food Bank Manager");
            }
        }

        private void btnAddtoPrint_Click(object sender, RoutedEventArgs e)
        {
            if (barcodes.Count < 30)
            {
                barcodes.Add(dImg);
                barcodeValues.Add(barcodeData);
            }

            if (barcodes.Count >= 30)
            {
                MessageBox.Show("Max barcode per sheet limit reached (30)", "Food Bank Manager");
            }

            txtNumBarcodes.Text = barcodes.Count.ToString();
            txtBarcodedata.Text = "";
        }


        private void bttnHome_Click(object sender, RoutedEventArgs e)
        {
            HomePage h = new HomePage(true);
            this.NavigationService.Navigate(h);
        }

        private void txtBarcodedata_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btnGenerateBarcode_Click(sender, e);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string result = "Barcodes\n";

            if (barcodeValues.Count > 0)
            {
                for (int i = 0; i < barcodeValues.Count; i++)
                {
                    result += (i + 1) + ") " + barcodeValues[i] + "\n";

                }

                MessageBox.Show(result, "Food Bank Manager"); 
            }
            else
            {
                MessageBox.Show("No Barcodes in Preview", "Food Bank Manager");
            }
        }






        #region Barcode Interface
        /// <summary>
        ///  Barcode interface for symbology layout.
        ///  Written by: Brad Barnhill
        /// </summary>
        interface IBarcode
        {
            string Encoded_Value
            {
                get;
            }//Encoded_Value

            string RawData
            {
                get;
            }//Raw_Data

            List<string> Errors
            {
                get;
            }//Errors

        }//interface
        /// <summary>
        ///  Blank encoding template
        ///  Written by: Brad Barnhill
        /// </summary>
        class Blank : BarcodeCommon, IBarcode
        {

            #region IBarcode Members

            public string Encoded_Value
            {
                get { throw new NotImplementedException(); }
            }

            #endregion
        }
        abstract class BarcodeCommon
        {
            protected string Raw_Data = "";
            protected List<string> _Errors = new List<string>();

            public string RawData
            {
                get { return this.Raw_Data; }
            }

            public List<string> Errors
            {
                get { return this._Errors; }
            }

            public void Error(string ErrorMessage)
            {
                this._Errors.Add(ErrorMessage);
                throw new Exception(ErrorMessage);
            }

            internal static bool CheckNumericOnly(string Data)
            {
                //This function takes a string of data and breaks it into parts and trys to do Int64.TryParse
                //This will verify that only numeric data is contained in the string passed in.  The complexity below
                //was done to ensure that the minimum number of interations and checks could be performed.

                //early check to see if the whole number can be parsed to improve efficency of this method
                long value = 0;
                if (Data != null)
                {
                    if (Int64.TryParse(Data, out value))
                        return true;
                }
                else
                {
                    return false;
                }

                //9223372036854775808 is the largest number a 64bit number(signed) can hold so ... make sure its less than that by one place
                int STRING_LENGTHS = 18;

                string temp = Data;
                string[] strings = new string[(Data.Length / STRING_LENGTHS) + ((Data.Length % STRING_LENGTHS == 0) ? 0 : 1)];

                int i = 0;
                while (i < strings.Length)
                {
                    if (temp.Length >= STRING_LENGTHS)
                    {
                        strings[i++] = temp.Substring(0, STRING_LENGTHS);
                        temp = temp.Substring(STRING_LENGTHS);
                    }//if
                    else
                        strings[i++] = temp.Substring(0);
                }

                foreach (string s in strings)
                {
                    if (!Int64.TryParse(s, out value))
                        return false;
                }//foreach

                return true;
            }//CheckNumericOnly
        }//BarcodeVariables abstract class
        #region Enums
        public enum TYPE : int { UNSPECIFIED, UPCA, UPCE, UPC_SUPPLEMENTAL_2DIGIT, UPC_SUPPLEMENTAL_5DIGIT, EAN13, EAN8, Interleaved2of5, Standard2of5, Industrial2of5, CODE39, CODE39Extended, CODE39_Mod43, Codabar, PostNet, BOOKLAND, ISBN, JAN13, MSI_Mod10, MSI_2Mod10, MSI_Mod11, MSI_Mod11_Mod10, Modified_Plessey, CODE11, USD8, UCC12, UCC13, LOGMARS, CODE128, CODE128A, CODE128B, CODE128C, ITF14, CODE93, TELEPEN, FIM, PHARMACODE };
        public enum SaveTypes : int { JPG, BMP, PNG, GIF, TIFF, UNSPECIFIED };
        public enum AlignmentPositions : int { CENTER, LEFT, RIGHT };
        public enum LabelPositions : int { TOPLEFT, TOPCENTER, TOPRIGHT, BOTTOMLEFT, BOTTOMCENTER, BOTTOMRIGHT };
        #endregion
        /// <summary>
        /// Generates a barcode image of a specified symbology from a string of data.
        /// </summary>
        public class Barcode : IDisposable
        {
            #region Variables
            private IBarcode ibarcode = new Blank();
            private string Raw_Data = "";
            private string Encoded_Value = "";
            private string _Country_Assigning_Manufacturer_Code = "N/A";
            private TYPE Encoded_Type = TYPE.UNSPECIFIED;
            private System.Drawing.Image _Encoded_Image = null;
            private System.Drawing.Color _ForeColor = System.Drawing.Color.Black;
            private System.Drawing.Color _BackColor = System.Drawing.Color.White;
            private int _Width = 200;
            private int _Height = 100;
            private string _XML = "";
            private ImageFormat _ImageFormat = ImageFormat.Jpeg;
            private Font _LabelFont = new Font("Microsoft Sans Serif", 10, System.Drawing.FontStyle.Bold);
            private AlignmentPositions _Alignment = AlignmentPositions.CENTER;
            private LabelPositions _LabelPosition = LabelPositions.BOTTOMCENTER;
            private RotateFlipType _RotateFlipType = RotateFlipType.RotateNoneFlipNone;
            #endregion

            #region Constructors
            /// <summary>
            /// Default constructor.  Does not populate the raw data.  MUST be done via the RawData property before encoding.
            /// </summary>
            public Barcode()
            {
                //constructor
            }//Barcode
             /// <summary>
             /// Constructor. Populates the raw data. No whitespace will be added before or after the barcode.
             /// </summary>
             /// <param name="data">String to be encoded.</param>
            public Barcode(string data)
            {
                //constructor
                this.Raw_Data = data;
            }//Barcode
            public Barcode(string data, TYPE iType)
            {
                this.Raw_Data = data;
                this.Encoded_Type = iType;
            }
            #endregion

            #region Properties
            /// <summary>
            /// Gets or sets the raw data to encode.
            /// </summary>
            public string RawData
            {
                get { return Raw_Data; }
                set { Raw_Data = value; }
            }//RawData
             /// <summary>
             /// Gets the encoded value.
             /// </summary>
            public string EncodedValue
            {
                get { return Encoded_Value; }
            }//EncodedValue
             /// <summary>
             /// Gets the Country that assigned the Manufacturer Code.
             /// </summary>
            public string Country_Assigning_Manufacturer_Code
            {
                get { return _Country_Assigning_Manufacturer_Code; }
            }//Country_Assigning_Manufacturer_Code
             /// <summary>
             /// Gets or sets the Encoded Type (ex. UPC-A, EAN-13 ... etc)
             /// </summary>
            public TYPE EncodedType
            {
                set { Encoded_Type = value; }
                get { return Encoded_Type; }
            }//EncodedType
             /// <summary>
             /// Gets the Image of the generated barcode.
             /// </summary>
            public System.Drawing.Image EncodedImage
            {
                get
                {
                    return _Encoded_Image;
                }
            }//EncodedImage
             /// <summary>
             /// Gets or sets the color of the bars. (Default is black)
             /// </summary>
            public System.Drawing.Color ForeColor
            {
                get { return this._ForeColor; }
                set { this._ForeColor = value; }
            }//ForeColor
             /// <summary>
             /// Gets or sets the background color. (Default is white)
             /// </summary>
            public System.Drawing.Color BackColor
            {
                get { return this._BackColor; }
                set { this._BackColor = value; }
            }//BackColor
             /// <summary>
             /// Gets or sets the label font. (Default is Microsoft Sans Serif, 10pt, Bold)
             /// </summary>
            public Font LabelFont
            {
                get { return this._LabelFont; }
                set { this._LabelFont = value; }
            }//LabelFont
             /// <summary>
             /// Gets or sets the location of the label in relation to the barcode. (BOTTOMCENTER is default)
             /// </summary>
            public LabelPositions LabelPosition
            {
                get { return _LabelPosition; }
                set { _LabelPosition = value; }
            }//LabelPosition
             /// <summary>
             /// Gets or sets the degree in which to rotate/flip the image.(No action is default)
             /// </summary>
            public RotateFlipType RotateFlipType
            {
                get { return _RotateFlipType; }
                set { _RotateFlipType = value; }
            }//RotatePosition
             /// <summary>
             /// Gets or sets the width of the image to be drawn. (Default is 300 pixels)
             /// </summary>
            public int Width
            {
                get { return _Width; }
                set { _Width = value; }
            }
            /// <summary>
            /// Gets or sets the height of the image to be drawn. (Default is 150 pixels)
            /// </summary>
            public int Height
            {
                get { return _Height; }
                set { _Height = value; }
            }
            /// <summary>
            /// Gets or sets whether a label should be drawn below the image. (Default is false)
            /// </summary>
            public bool IncludeLabel
            {
                get;
                set;
            }

            /// <summary>
            /// Alternate label to be displayed.  (IncludeLabel must be set to true as well)
            /// </summary>
            public String AlternateLabel
            {
                get;
                set;
            }

            /// <summary>
            /// Gets or sets the amount of time in milliseconds that it took to encode and draw the barcode.
            /// </summary>
            public double EncodingTime
            {
                get;
                set;
            }
            /// <summary>
            /// Gets the XML representation of the Barcode data and image.
            /// </summary>
            public string XML
            {
                get { return _XML; }
            }
            /// <summary>
            /// Gets or sets the image format to use when encoding and returning images. (Jpeg is default)
            /// </summary>
            public ImageFormat ImageFormat
            {
                get { return _ImageFormat; }
                set { _ImageFormat = value; }
            }
            /// <summary>
            /// Gets the list of errors encountered.
            /// </summary>
            public List<string> Errors
            {
                get { return this.ibarcode.Errors; }
            }
            /// <summary>
            /// Gets or sets the alignment of the barcode inside the image. (Not for Postnet or ITF-14)
            /// </summary>
            public AlignmentPositions Alignment
            {
                get;
                set;
            }//Alignment
             /// <summary>
             /// Gets a byte array representation of the encoded image. (Used for Crystal Reports)
             /// </summary>
            public byte[] Encoded_Image_Bytes
            {
                get
                {
                    if (_Encoded_Image == null)
                        return null;

                    using (MemoryStream ms = new MemoryStream())
                    {
                        _Encoded_Image.Save(ms, _ImageFormat);
                        return ms.ToArray();
                    }//using
                }
            }
            /// <summary>
            /// Gets the assembly version information.
            /// </summary>
            public static Version Version
            {
                get { return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version; }
            }
            #endregion

            /// <summary>
            /// Represents the size of an image in real world coordinates (millimeters or inches).
            /// </summary>
            public class ImageSize
            {
                public ImageSize(double width, double height, bool metric)
                {
                    Width = width;
                    Height = height;
                    Metric = metric;
                }

                public double Width { get; set; }
                public double Height { get; set; }
                public bool Metric { get; set; }
            }

            #region Functions
            #region General Encode
            /// <summary>
            /// Encodes the raw data into binary form representing bars and spaces.  Also generates an Image of the barcode.
            /// </summary>
            /// <param name="iType">Type of encoding to use.</param>
            /// <param name="StringToEncode">Raw data to encode.</param>
            /// <param name="Width">Width of the resulting barcode.(pixels)</param>
            /// <param name="Height">Height of the resulting barcode.(pixels)</param>
            /// <returns>Image representing the barcode.</returns>
            public System.Drawing.Image Encode(TYPE iType, string StringToEncode, int Width, int Height)
            {
                this.Width = Width;
                this.Height = Height;
                return Encode(iType, StringToEncode);
            }

            //Encode(TYPE, string, int, int)
            /// <summary>
            /// Encodes the raw data into binary form representing bars and spaces.  Also generates an Image of the barcode.
            /// </summary>
            /// <param name="iType">Type of encoding to use.</param>
            /// <param name="StringToEncode">Raw data to encode.</param>
            /// <param name="DrawColor">Foreground color</param>
            /// <param name="BackColor">Background color</param>
            /// <param name="Width">Width of the resulting barcode.(pixels)</param>
            /// <param name="Height">Height of the resulting barcode.(pixels)</param>
            /// <returns>Image representing the barcode.</returns>
            public System.Drawing.Image Encode(TYPE iType, string StringToEncode, System.Drawing.Color ForeColor, System.Drawing.Color BackColor, int Width, int Height)
            {
                this.Width = Width;
                this.Height = Height;
                return Encode(iType, StringToEncode, ForeColor, BackColor);
            }

            //Encode(TYPE, string, Color, Color, int, int)
            /// <summary>
            /// Encodes the raw data into binary form representing bars and spaces.  Also generates an Image of the barcode.
            /// </summary>
            /// <param name="iType">Type of encoding to use.</param>
            /// <param name="StringToEncode">Raw data to encode.</param>
            /// <param name="DrawColor">Foreground color</param>
            /// <param name="BackColor">Background color</param>
            /// <returns>Image representing the barcode.</returns>
            public System.Drawing.Image Encode(TYPE iType, string StringToEncode, System.Drawing.Color ForeColor, System.Drawing.Color BackColor)
            {
                this.BackColor = BackColor;
                this.ForeColor = ForeColor;
                return Encode(iType, StringToEncode);
            }
            //(Image)Encode(Type, string, Color, Color)
            /// <summary>
            /// Encodes the raw data into binary form representing bars and spaces.  Also generates an Image of the barcode.
            /// </summary>
            /// <param name="iType">Type of encoding to use.</param>
            /// <param name="StringToEncode">Raw data to encode.</param>
            /// <returns>Image representing the barcode.</returns>
            public System.Drawing.Image Encode(TYPE iType, string StringToEncode)
            {
                Raw_Data = StringToEncode;
                return Encode(iType);
            }//(Image)Encode(TYPE, string)
             /// <summary>
             /// Encodes the raw data into binary form representing bars and spaces.  Also generates an Image of the barcode.
             /// </summary>
             /// <param name="iType">Type of encoding to use.</param>
            internal System.Drawing.Image Encode(TYPE iType)
            {
                Encoded_Type = iType;
                return Encode();
            }//Encode()
             /// <summary>
             /// Encodes the raw data into binary form representing bars and spaces.
             /// </summary>
            internal System.Drawing.Image Encode()
            {
                ibarcode.Errors.Clear();

                DateTime dtStartTime = DateTime.Now;

                //make sure there is something to encode
                if (Raw_Data.Trim() == "")
                    throw new Exception("EENCODE-1: Input data not allowed to be blank.");

                if (this.EncodedType == TYPE.UNSPECIFIED)
                    throw new Exception("EENCODE-2: Symbology type not allowed to be unspecified.");

                this.Encoded_Value = "";
                this._Country_Assigning_Manufacturer_Code = "N/A";

                ibarcode = new Code39(Raw_Data);

                this.Encoded_Value = ibarcode.Encoded_Value;
                this.Raw_Data = ibarcode.RawData;

                _Encoded_Image = (System.Drawing.Image)Generate_Image();

                this.EncodedImage.RotateFlip(this.RotateFlipType);

                //_XML = GetXML();

                this.EncodingTime = ((TimeSpan)(DateTime.Now - dtStartTime)).TotalMilliseconds;

                return EncodedImage;
            }//Encode
            #endregion

            #region Image Functions
            /// <summary>
            /// Gets a bitmap representation of the encoded data.
            /// </summary>
            /// <returns>Bitmap of encoded value.</returns>
            private Bitmap Generate_Image()
            {
                if (Encoded_Value == "") throw new Exception("EGENERATE_IMAGE-1: Must be encoded first.");
                Bitmap b = null;

                DateTime dtStartTime = DateTime.Now;

                switch (this.Encoded_Type)
                {
                    case TYPE.ITF14:
                        {
                            b = new Bitmap(Width, Height);

                            int bearerwidth = (int)((b.Width) / 12.05);
                            int iquietzone = Convert.ToInt32(b.Width * 0.05);
                            int iBarWidth = (b.Width - (bearerwidth * 2) - (iquietzone * 2)) / Encoded_Value.Length;
                            int shiftAdjustment = ((b.Width - (bearerwidth * 2) - (iquietzone * 2)) % Encoded_Value.Length) / 2;

                            if (iBarWidth <= 0 || iquietzone <= 0)
                                throw new Exception("EGENERATE_IMAGE-3: Image size specified not large enough to draw image. (Bar size determined to be less than 1 pixel or quiet zone determined to be less than 1 pixel)");

                            //draw image
                            int pos = 0;

                            using (Graphics g = Graphics.FromImage(b))
                            {
                                //fill background
                                g.Clear(BackColor);

                                //lines are fBarWidth wide so draw the appropriate color line vertically
                                using (System.Drawing.Pen pen = new System.Drawing.Pen(ForeColor, iBarWidth))
                                {
                                    pen.Alignment = PenAlignment.Right;

                                    while (pos < Encoded_Value.Length)
                                    {
                                        //draw the appropriate color line vertically
                                        if (Encoded_Value[pos] == '1')
                                            g.DrawLine(pen, new System.Drawing.Point((pos * iBarWidth) + shiftAdjustment + bearerwidth + iquietzone, 0), new System.Drawing.Point((pos * iBarWidth) + shiftAdjustment + bearerwidth + iquietzone, Height));

                                        pos++;
                                    }//while

                                    //bearer bars
                                    pen.Width = (float)b.Height / 8;
                                    pen.Color = ForeColor;
                                    pen.Alignment = PenAlignment.Center;
                                    g.DrawLine(pen, new System.Drawing.Point(0, 0), new System.Drawing.Point(b.Width, 0));//top
                                    g.DrawLine(pen, new System.Drawing.Point(0, b.Height), new System.Drawing.Point(b.Width, b.Height));//bottom
                                    g.DrawLine(pen, new System.Drawing.Point(0, 0), new System.Drawing.Point(0, b.Height));//left
                                    g.DrawLine(pen, new System.Drawing.Point(b.Width, 0), new System.Drawing.Point(b.Width, b.Height));//right
                                }//using
                            }//using

                            if (IncludeLabel)
                                Label_ITF14((System.Drawing.Image)b);

                            break;
                        }//case
                    default:
                        {
                            b = new Bitmap(Width, Height);
                            int iBarWidth = Width / Encoded_Value.Length;
                            int shiftAdjustment = 0;
                            int iBarWidthModifier = 1;

                            if (this.Encoded_Type == TYPE.PostNet)
                                iBarWidthModifier = 2;

                            //set alignment
                            switch (Alignment)
                            {
                                case AlignmentPositions.CENTER:
                                    shiftAdjustment = (Width % Encoded_Value.Length) / 2;
                                    break;
                                case AlignmentPositions.LEFT:
                                    shiftAdjustment = 0;
                                    break;
                                case AlignmentPositions.RIGHT:
                                    shiftAdjustment = (Width % Encoded_Value.Length);
                                    break;
                                default:
                                    shiftAdjustment = (Width % Encoded_Value.Length) / 2;
                                    break;
                            }//switch

                            if (iBarWidth <= 0)
                                throw new Exception("EGENERATE_IMAGE-2: Image size specified not large enough to draw image. (Bar size determined to be less than 1 pixel)");

                            //draw image
                            int pos = 0;
                            int halfBarWidth = (int)(iBarWidth * 0.5);

                            using (Graphics g = Graphics.FromImage(b))
                            {
                                //clears the image and colors the entire background
                                g.Clear(BackColor);

                                //lines are fBarWidth wide so draw the appropriate color line vertically
                                using (System.Drawing.Pen backpen = new System.Drawing.Pen(BackColor, iBarWidth / iBarWidthModifier))
                                {
                                    using (System.Drawing.Pen pen = new System.Drawing.Pen(ForeColor, iBarWidth / iBarWidthModifier))
                                    {
                                        while (pos < Encoded_Value.Length)
                                        {
                                            if (this.Encoded_Type == TYPE.PostNet)
                                            {
                                                //draw half bars in postnet
                                                if (Encoded_Value[pos] == '0')
                                                    g.DrawLine(pen, new System.Drawing.Point(pos * iBarWidth + shiftAdjustment + halfBarWidth, Height), new System.Drawing.Point(pos * iBarWidth + shiftAdjustment + halfBarWidth, Height / 2));
                                                else
                                                    g.DrawLine(pen, new System.Drawing.Point(pos * iBarWidth + shiftAdjustment + halfBarWidth, Height), new System.Drawing.Point(pos * iBarWidth + shiftAdjustment + halfBarWidth, 0));
                                            }//if
                                            else
                                            {
                                                if (Encoded_Value[pos] == '1')
                                                    g.DrawLine(pen, new System.Drawing.Point(pos * iBarWidth + shiftAdjustment + halfBarWidth, 0), new System.Drawing.Point(pos * iBarWidth + shiftAdjustment + halfBarWidth, Height));
                                            }
                                            pos++;
                                        }//while
                                    }//using
                                }//using
                            }//using
                            if (IncludeLabel)
                            {
                                //if (this.EncodedType != TYPE.UPCA)
                                Label_Generic((System.Drawing.Image)b);
                                //else
                                //    Label_UPCA((Image)b);
                            }//if

                            break;
                        }//case
                }//switch

                _Encoded_Image = (System.Drawing.Image)b;

                this.EncodingTime += ((TimeSpan)(DateTime.Now - dtStartTime)).TotalMilliseconds;

                return b;
            }//Generate_Image
             /// <summary>
             /// Gets the bytes that represent the image.
             /// </summary>
             /// <param name="savetype">File type to put the data in before returning the bytes.</param>
             /// <returns>Bytes representing the encoded image.</returns>
            public byte[] GetImageData(SaveTypes savetype)
            {
                byte[] imageData = null;

                try
                {
                    if (_Encoded_Image != null)
                    {
                        //Save the image to a memory stream so that we can get a byte array!      
                        using (MemoryStream ms = new MemoryStream())
                        {
                            SaveImage(ms, savetype);
                            imageData = ms.ToArray();
                            ms.Flush();
                            ms.Close();
                        }//using
                    }//if
                }//try
                catch (Exception ex)
                {
                    throw new Exception("EGETIMAGEDATA-1: Could not retrieve image data. " + ex.Message);
                }//catch  
                return imageData;
            }
            /// <summary>
            /// Saves an encoded image to a specified file and type.
            /// </summary>
            /// <param name="Filename">Filename to save to.</param>
            /// <param name="FileType">Format to use.</param>
            public void SaveImage(string Filename, SaveTypes FileType)
            {
                try
                {
                    if (_Encoded_Image != null)
                    {
                        System.Drawing.Imaging.ImageFormat imageformat;
                        switch (FileType)
                        {
                            case SaveTypes.BMP: imageformat = System.Drawing.Imaging.ImageFormat.Bmp; break;
                            case SaveTypes.GIF: imageformat = System.Drawing.Imaging.ImageFormat.Gif; break;
                            case SaveTypes.JPG: imageformat = System.Drawing.Imaging.ImageFormat.Jpeg; break;
                            case SaveTypes.PNG: imageformat = System.Drawing.Imaging.ImageFormat.Png; break;
                            case SaveTypes.TIFF: imageformat = System.Drawing.Imaging.ImageFormat.Tiff; break;
                            default: imageformat = ImageFormat; break;
                        }//switch
                        ((Bitmap)_Encoded_Image).Save(Filename, imageformat);
                    }//if
                }//try
                catch (Exception ex)
                {
                    throw new Exception("ESAVEIMAGE-1: Could not save image.\n\n=======================\n\n" + ex.Message);
                }//catch
            }//SaveImage(string, SaveTypes)
             /// <summary>
             /// Saves an encoded image to a specified stream.
             /// </summary>
             /// <param name="stream">Stream to write image to.</param>
             /// <param name="FileType">Format to use.</param>
            public void SaveImage(Stream stream, SaveTypes FileType)
            {
                try
                {
                    if (_Encoded_Image != null)
                    {
                        System.Drawing.Imaging.ImageFormat imageformat;
                        switch (FileType)
                        {
                            case SaveTypes.BMP: imageformat = System.Drawing.Imaging.ImageFormat.Bmp; break;
                            case SaveTypes.GIF: imageformat = System.Drawing.Imaging.ImageFormat.Gif; break;
                            case SaveTypes.JPG: imageformat = System.Drawing.Imaging.ImageFormat.Jpeg; break;
                            case SaveTypes.PNG: imageformat = System.Drawing.Imaging.ImageFormat.Png; break;
                            case SaveTypes.TIFF: imageformat = System.Drawing.Imaging.ImageFormat.Tiff; break;
                            default: imageformat = ImageFormat; break;
                        }//switch
                        ((Bitmap)_Encoded_Image).Save(stream, imageformat);
                    }//if
                }//try
                catch (Exception ex)
                {
                    throw new Exception("ESAVEIMAGE-2: Could not save image.\n\n=======================\n\n" + ex.Message);
                }//catch
            }//SaveImage(Stream, SaveTypes)

            /// <summary>
            /// Returns the size of the EncodedImage in real world coordinates (millimeters or inches).
            /// </summary>
            /// <param name="Metric">Millimeters if true, otherwise Inches.</param>
            /// <returns></returns>
            public ImageSize GetSizeOfImage(bool Metric)
            {
                double Width = 0;
                double Height = 0;
                if (this.EncodedImage != null && this.EncodedImage.Width > 0 && this.EncodedImage.Height > 0)
                {
                    double MillimetersPerInch = 25.4;
                    using (Graphics g = Graphics.FromImage(this.EncodedImage))
                    {
                        Width = 150; //this.EncodedImage.Width / g.DpiX;
                        Height = 150; //this.EncodedImage.Height / g.DpiY;

                        if (Metric)
                        {
                            Width *= MillimetersPerInch;
                            Height *= MillimetersPerInch;
                        }//if
                    }//using
                }//if

                return new ImageSize(Width, Height, Metric);
            }
            #endregion

            #region Label Generation
            private System.Drawing.Image Label_ITF14(System.Drawing.Image img)
            {
                try
                {
                    System.Drawing.Font font = this.LabelFont;

                    using (Graphics g = Graphics.FromImage(img))
                    {
                        g.DrawImage(img, (float)0, (float)0);

                        g.SmoothingMode = SmoothingMode.HighQuality;
                        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                        g.CompositingQuality = CompositingQuality.HighQuality;

                        //color a white box at the bottom of the barcode to hold the string of data
                        g.FillRectangle(new SolidBrush(this.BackColor), new System.Drawing.Rectangle(0, img.Height - (font.Height - 2), img.Width, font.Height));

                        //draw datastring under the barcode image
                        StringFormat f = new StringFormat();
                        f.Alignment = StringAlignment.Center;
                        g.DrawString(AlternateLabel == null ? RawData : AlternateLabel, font, new SolidBrush(ForeColor), (float)(img.Width / 2), img.Height - font.Height + 1, f);

                        System.Drawing.Pen pen = new System.Drawing.Pen(ForeColor, (float)img.Height / 16);
                        pen.Alignment = PenAlignment.Inset;
                        g.DrawLine(pen, new System.Drawing.Point(0, img.Height - font.Height - 2), new System.Drawing.Point(img.Width, img.Height - font.Height - 2));//bottom

                        g.Save();
                    }//using
                    return img;
                }//try
                catch (Exception ex)
                {
                    throw new Exception("ELABEL_ITF14-1: " + ex.Message);
                }//catch
            }
            private System.Drawing.Image Label_Generic(System.Drawing.Image img)
            {
                try
                {
                    System.Drawing.Font font = this.LabelFont;

                    using (Graphics g = Graphics.FromImage(img))
                    {
                        g.DrawImage(img, (float)0, (float)0);

                        g.SmoothingMode = SmoothingMode.HighQuality;
                        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                        g.CompositingQuality = CompositingQuality.HighQuality;

                        StringFormat f = new StringFormat();
                        f.Alignment = StringAlignment.Near;
                        f.LineAlignment = StringAlignment.Near;
                        int LabelX = 0;
                        int LabelY = 0;

                        switch (LabelPosition)
                        {
                            case LabelPositions.BOTTOMCENTER:
                                LabelX = img.Width / 2;
                                LabelY = img.Height - (font.Height);
                                f.Alignment = StringAlignment.Center;
                                break;
                            case LabelPositions.BOTTOMLEFT:
                                LabelX = 0;
                                LabelY = img.Height - (font.Height);
                                f.Alignment = StringAlignment.Near;
                                break;
                            case LabelPositions.BOTTOMRIGHT:
                                LabelX = img.Width;
                                LabelY = img.Height - (font.Height);
                                f.Alignment = StringAlignment.Far;
                                break;
                            case LabelPositions.TOPCENTER:
                                LabelX = img.Width / 2;
                                LabelY = 0;
                                f.Alignment = StringAlignment.Center;
                                break;
                            case LabelPositions.TOPLEFT:
                                LabelX = img.Width;
                                LabelY = 0;
                                f.Alignment = StringAlignment.Near;
                                break;
                            case LabelPositions.TOPRIGHT:
                                LabelX = img.Width;
                                LabelY = 0;
                                f.Alignment = StringAlignment.Far;
                                break;
                        }//switch

                        //color a background color box at the bottom of the barcode to hold the string of data
                        g.FillRectangle(new SolidBrush(BackColor), new RectangleF((float)0, (float)LabelY, (float)img.Width, (float)font.Height));

                        //draw datastring under the barcode image
                        g.DrawString(AlternateLabel == null ? RawData : AlternateLabel, font, new SolidBrush(ForeColor), new RectangleF((float)0, (float)LabelY, (float)img.Width, (float)font.Height), f);

                        g.Save();
                    }//using
                    return img;
                }//try
                catch (Exception ex)
                {
                    throw new Exception("ELABEL_GENERIC-1: " + ex.Message);
                }//catch
            }//Label_Generic

            /// <summary>
            /// Draws Label for UPC-A barcodes (NOT COMPLETE)
            /// </summary>
            /// <param name="img"></param>
            /// <returns></returns>
            private System.Drawing.Image Label_UPCA(System.Drawing.Image img)
            {
                try
                {
                    int iBarWidth = Width / Encoded_Value.Length;
                    int shiftAdjustment = 0;

                    //set alignment
                    switch (Alignment)
                    {
                        case AlignmentPositions.CENTER:
                            shiftAdjustment = (Width % Encoded_Value.Length) / 2;
                            break;
                        case AlignmentPositions.LEFT:
                            shiftAdjustment = 0;
                            break;
                        case AlignmentPositions.RIGHT:
                            shiftAdjustment = (Width % Encoded_Value.Length);
                            break;
                        default:
                            shiftAdjustment = (Width % Encoded_Value.Length) / 2;
                            break;
                    }//switch

                    System.Drawing.Font font = new System.Drawing.Font("OCR A Extended", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0))); ;

                    using (Graphics g = Graphics.FromImage(img))
                    {
                        g.DrawImage(img, (float)0, (float)0);

                        g.SmoothingMode = SmoothingMode.HighQuality;
                        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                        g.CompositingQuality = CompositingQuality.HighQuality;

                        //draw datastring under the barcode image
                        RectangleF rect = new RectangleF((iBarWidth * 3) + shiftAdjustment, this.Height - (int)(this.Height * 0.1), (iBarWidth * 43), (int)(this.Height * 0.1));
                        g.FillRectangle(new SolidBrush(System.Drawing.Color.Yellow), rect.X, rect.Y, rect.Width, rect.Height);
                        g.DrawString(this.RawData.Substring(1, 5), font, new SolidBrush(this.ForeColor), rect.X, rect.Y);

                        g.Save();
                    }//using
                    return img;
                }//try
                catch (Exception ex)
                {
                    throw new Exception("ELABEL_UPCA-1: " + ex.Message);
                }//catch
            }//Label_UPCA
            #endregion
            #endregion

            #region Misc
            //private string GetXML()
            //{
            //    if (EncodedValue == "")
            //        throw new Exception("EGETXML-1: Could not retrieve XML due to the barcode not being encoded first.  Please call Encode first.");
            //    else
            //    {
            //        try
            //        {
            //            using (BarcodeXML xml = new BarcodeXML())
            //            {
            //                BarcodeXML.BarcodeRow row = xml.Barcode.NewBarcodeRow();
            //                row.Type = EncodedType.ToString();
            //                row.RawData = RawData;
            //                row.EncodedValue = EncodedValue;
            //                row.EncodingTime = EncodingTime;
            //                row.IncludeLabel = IncludeLabel;
            //                row.Forecolor = ColorTranslator.ToHtml(ForeColor);
            //                row.Backcolor = ColorTranslator.ToHtml(BackColor);
            //                row.CountryAssigningManufacturingCode = Country_Assigning_Manufacturer_Code;
            //                row.ImageWidth = Width;
            //                row.ImageHeight = Height;
            //                row.RotateFlipType = this.RotateFlipType;
            //                row.LabelPosition = (int)this.LabelPosition;
            //                row.LabelFont = this.LabelFont.ToString();
            //                row.ImageFormat = this.ImageFormat.ToString();
            //                row.Alignment = (int)this.Alignment;

            //                //get image in base 64
            //                using (MemoryStream ms = new MemoryStream())
            //                {
            //                    EncodedImage.Save(ms, ImageFormat);
            //                    row.Image = Convert.ToBase64String(ms.ToArray(), Base64FormattingOptions.None);
            //                }//using

            //                xml.Barcode.AddBarcodeRow(row);

            //                StringWriter sw = new StringWriter();
            //                xml.WriteXml(sw, XmlWriteMode.WriteSchema);
            //                return sw.ToString();
            //            }//using
            //        }//try
            //        catch (Exception ex)
            //        {
            //            throw new Exception("EGETXML-2: " + ex.Message);
            //        }//catch
            //    }//else
            //}
            //public static Image GetImageFromXML(BarcodeXML internalXML)
            //{
            //    try
            //    {
            //        //converting the base64 string to byte array
            //        Byte[] imageContent = new Byte[internalXML.Barcode[0].Image.Length];

            //        //loading it to memory stream and then to image object
            //        using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(internalXML.Barcode[0].Image)))
            //        {
            //            return Image.FromStream(ms);
            //        }//using
            //    }//try
            //    catch (Exception ex)
            //    {
            //        throw new Exception("EGETIMAGEFROMXML-1: " + ex.Message);
            //    }//catch
            //}
            #endregion

            #region Static Methods
            /// <summary>
            /// Encodes the raw data into binary form representing bars and spaces.  Also generates an Image of the barcode.
            /// </summary>
            /// <param name="iType">Type of encoding to use.</param>
            /// <param name="Data">Raw data to encode.</param>
            /// <returns>Image representing the barcode.</returns>
            public static System.Drawing.Image DoEncode(TYPE iType, string Data)
            {
                using (Barcode b = new Barcode())
                {
                    return b.Encode(iType, Data);
                }//using
            }
            /// <summary>
            /// Encodes the raw data into binary form representing bars and spaces.  Also generates an Image of the barcode.
            /// </summary>
            /// <param name="iType">Type of encoding to use.</param>
            /// <param name="Data">Raw data to encode.</param>
            /// <param name="XML">XML representation of the data and the image of the barcode.</param>
            /// <returns>Image representing the barcode.</returns>
            public static System.Drawing.Image DoEncode(TYPE iType, string Data, ref string XML)
            {
                using (Barcode b = new Barcode())
                {
                    System.Drawing.Image i = b.Encode(iType, Data);
                    XML = b.XML;
                    return i;
                }//using
            }
            /// <summary>
            /// Encodes the raw data into binary form representing bars and spaces.  Also generates an Image of the barcode.
            /// </summary>
            /// <param name="iType">Type of encoding to use.</param>
            /// <param name="Data">Raw data to encode.</param>
            /// <param name="IncludeLabel">Include the label at the bottom of the image with data encoded.</param>
            /// <returns>Image representing the barcode.</returns>
            public static System.Drawing.Image DoEncode(TYPE iType, string Data, bool IncludeLabel)
            {
                using (Barcode b = new Barcode())
                {
                    b.IncludeLabel = IncludeLabel;
                    return b.Encode(iType, Data);
                }//using
            }
            /// <summary>
            /// Encodes the raw data into binary form representing bars and spaces.  Also generates an Image of the barcode.
            /// </summary>
            /// <param name="iType">Type of encoding to use.</param>
            /// <param name="data">Raw data to encode.</param>
            /// <param name="IncludeLabel">Include the label at the bottom of the image with data encoded.</param>
            /// <param name="Width">Width of the resulting barcode.(pixels)</param>
            /// <param name="Height">Height of the resulting barcode.(pixels)</param>
            /// <returns>Image representing the barcode.</returns>
            public static System.Drawing.Image DoEncode(TYPE iType, string Data, bool IncludeLabel, int Width, int Height)
            {
                using (Barcode b = new Barcode())
                {
                    b.IncludeLabel = IncludeLabel;
                    return b.Encode(iType, Data, Width, Height);
                }//using
            }
            /// <summary>
            /// Encodes the raw data into binary form representing bars and spaces.  Also generates an Image of the barcode.
            /// </summary>
            /// <param name="iType">Type of encoding to use.</param>
            /// <param name="Data">Raw data to encode.</param>
            /// <param name="IncludeLabel">Include the label at the bottom of the image with data encoded.</param>
            /// <param name="DrawColor">Foreground color</param>
            /// <param name="BackColor">Background color</param>
            /// <returns>Image representing the barcode.</returns>
            public static System.Drawing.Image DoEncode(TYPE iType, string Data, bool IncludeLabel, System.Drawing.Color DrawColor, System.Drawing.Color BackColor)
            {
                using (Barcode b = new Barcode())
                {
                    b.IncludeLabel = IncludeLabel;
                    return b.Encode(iType, Data, DrawColor, BackColor);
                }//using
            }
            /// <summary>
            /// Encodes the raw data into binary form representing bars and spaces.  Also generates an Image of the barcode.
            /// </summary>
            /// <param name="iType">Type of encoding to use.</param>
            /// <param name="Data">Raw data to encode.</param>
            /// <param name="IncludeLabel">Include the label at the bottom of the image with data encoded.</param>
            /// <param name="DrawColor">Foreground color</param>
            /// <param name="BackColor">Background color</param>
            /// <param name="Width">Width of the resulting barcode.(pixels)</param>
            /// <param name="Height">Height of the resulting barcode.(pixels)</param>
            /// <returns>Image representing the barcode.</returns>
            public static System.Drawing.Image DoEncode(TYPE iType, string Data, bool IncludeLabel, System.Drawing.Color DrawColor, System.Drawing.Color BackColor, int Width, int Height)
            {
                using (Barcode b = new Barcode())
                {
                    b.IncludeLabel = IncludeLabel;
                    return b.Encode(iType, Data, DrawColor, BackColor, Width, Height);
                }//using
            }
            /// <summary>
            /// Encodes the raw data into binary form representing bars and spaces.  Also generates an Image of the barcode.
            /// </summary>
            /// <param name="iType">Type of encoding to use.</param>
            /// <param name="Data">Raw data to encode.</param>
            /// <param name="IncludeLabel">Include the label at the bottom of the image with data encoded.</param>
            /// <param name="DrawColor">Foreground color</param>
            /// <param name="BackColor">Background color</param>
            /// <param name="Width">Width of the resulting barcode.(pixels)</param>
            /// <param name="Height">Height of the resulting barcode.(pixels)</param>
            /// <param name="XML">XML representation of the data and the image of the barcode.</param>
            /// <returns>Image representing the barcode.</returns>
            public static System.Drawing.Image DoEncode(TYPE iType, string Data, bool IncludeLabel, System.Drawing.Color DrawColor, System.Drawing.Color BackColor, int Width, int Height, ref string XML)
            {
                using (Barcode b = new Barcode())
                {
                    b.IncludeLabel = IncludeLabel;
                    System.Drawing.Image i = b.Encode(iType, Data, DrawColor, BackColor, Width, Height);
                    XML = b.XML;
                    return i;
                }//using
            }

            #endregion

            #region IDisposable Members

            public void Dispose()
            {
                try
                {
                }//try
                catch (Exception ex)
                {
                    throw new Exception("EDISPOSE-1: " + ex.Message);
                }//catch
            }

            #endregion
        }//Barcode Class
        /// <summary>
        ///  Code 39 encoding
        ///  Written by: Brad Barnhill
        /// </summary>
        class Code39 : BarcodeCommon, IBarcode
        {
            private System.Collections.Hashtable C39_Code = new System.Collections.Hashtable(); //is initialized by init_Code39()
            private System.Collections.Hashtable ExtC39_Translation = new System.Collections.Hashtable();
            private bool _AllowExtended = false;
            private bool _EnableChecksum = false;

            /// <summary>
            /// Encodes with Code39.
            /// </summary>
            /// <param name="input">Data to encode.</param>
            public Code39(string input)
            {
                Raw_Data = input;
            }//Code39

            /// <summary>
            /// Encodes with Code39.
            /// </summary>
            /// <param name="input">Data to encode.</param>
            /// <param name="AllowExtended">Allow Extended Code 39 (Full Ascii mode).</param>
            public Code39(string input, bool AllowExtended)
            {
                Raw_Data = input;
                _AllowExtended = AllowExtended;
            }

            /// <summary>
            /// Encodes with Code39.
            /// </summary>
            /// <param name="input">Data to encode.</param>
            /// <param name="AllowExtended">Allow Extended Code 39 (Full Ascii mode).</param>
            /// <param name="EnableChecksum">Whether to calculate the Mod 43 checksum and encode it into the barcode</param>
            public Code39(string input, bool AllowExtended, bool EnableChecksum)
            {
                Raw_Data = input;
                _AllowExtended = AllowExtended;
                _EnableChecksum = EnableChecksum;
            }

            /// <summary>
            /// Encode the raw data using the Code 39 algorithm.
            /// </summary>
            private string Encode_Code39()
            {
                this.init_Code39();
                this.init_ExtendedCode39();

                string strNoAstr = Raw_Data.Replace("*", "");
                string strFormattedData = "*" + strNoAstr + (_EnableChecksum ? getChecksumChar(strNoAstr).ToString() : String.Empty) + "*";

                if (_AllowExtended)
                    InsertExtendedCharsIfNeeded(ref strFormattedData);

                string result = "";
                //foreach (char c in this.FormattedData)
                foreach (char c in strFormattedData)
                {
                    try
                    {
                        result += C39_Code[c].ToString();
                        result += "0";//whitespace
                    }//try
                    catch
                    {
                        if (_AllowExtended)
                            Error("EC39-1: Invalid data.");
                        else
                            Error("EC39-1: Invalid data. (Try using Extended Code39)");
                    }//catch
                }//foreach

                result = result.Substring(0, result.Length - 1);

                //clear the hashtable so it no longer takes up memory
                this.C39_Code.Clear();

                return result;
            }//Encode_Code39
            private void init_Code39()
            {
                C39_Code.Clear();
                C39_Code.Add('0', "101001101101");
                C39_Code.Add('1', "110100101011");
                C39_Code.Add('2', "101100101011");
                C39_Code.Add('3', "110110010101");
                C39_Code.Add('4', "101001101011");
                C39_Code.Add('5', "110100110101");
                C39_Code.Add('6', "101100110101");
                C39_Code.Add('7', "101001011011");
                C39_Code.Add('8', "110100101101");
                C39_Code.Add('9', "101100101101");
                C39_Code.Add('A', "110101001011");
                C39_Code.Add('B', "101101001011");
                C39_Code.Add('C', "110110100101");
                C39_Code.Add('D', "101011001011");
                C39_Code.Add('E', "110101100101");
                C39_Code.Add('F', "101101100101");
                C39_Code.Add('G', "101010011011");
                C39_Code.Add('H', "110101001101");
                C39_Code.Add('I', "101101001101");
                C39_Code.Add('J', "101011001101");
                C39_Code.Add('K', "110101010011");
                C39_Code.Add('L', "101101010011");
                C39_Code.Add('M', "110110101001");
                C39_Code.Add('N', "101011010011");
                C39_Code.Add('O', "110101101001");
                C39_Code.Add('P', "101101101001");
                C39_Code.Add('Q', "101010110011");
                C39_Code.Add('R', "110101011001");
                C39_Code.Add('S', "101101011001");
                C39_Code.Add('T', "101011011001");
                C39_Code.Add('U', "110010101011");
                C39_Code.Add('V', "100110101011");
                C39_Code.Add('W', "110011010101");
                C39_Code.Add('X', "100101101011");
                C39_Code.Add('Y', "110010110101");
                C39_Code.Add('Z', "100110110101");
                C39_Code.Add('-', "100101011011");
                C39_Code.Add('.', "110010101101");
                C39_Code.Add(' ', "100110101101");
                C39_Code.Add('$', "100100100101");
                C39_Code.Add('/', "100100101001");
                C39_Code.Add('+', "100101001001");
                C39_Code.Add('%', "101001001001");
                C39_Code.Add('*', "100101101101");
            }//init_Code39
            private void init_ExtendedCode39()
            {
                ExtC39_Translation.Clear();
                ExtC39_Translation.Add(Convert.ToChar(0).ToString(), "%U");
                ExtC39_Translation.Add(Convert.ToChar(1).ToString(), "$A");
                ExtC39_Translation.Add(Convert.ToChar(2).ToString(), "$B");
                ExtC39_Translation.Add(Convert.ToChar(3).ToString(), "$C");
                ExtC39_Translation.Add(Convert.ToChar(4).ToString(), "$D");
                ExtC39_Translation.Add(Convert.ToChar(5).ToString(), "$E");
                ExtC39_Translation.Add(Convert.ToChar(6).ToString(), "$F");
                ExtC39_Translation.Add(Convert.ToChar(7).ToString(), "$G");
                ExtC39_Translation.Add(Convert.ToChar(8).ToString(), "$H");
                ExtC39_Translation.Add(Convert.ToChar(9).ToString(), "$I");
                ExtC39_Translation.Add(Convert.ToChar(10).ToString(), "$J");
                ExtC39_Translation.Add(Convert.ToChar(11).ToString(), "$K");
                ExtC39_Translation.Add(Convert.ToChar(12).ToString(), "$L");
                ExtC39_Translation.Add(Convert.ToChar(13).ToString(), "$M");
                ExtC39_Translation.Add(Convert.ToChar(14).ToString(), "$N");
                ExtC39_Translation.Add(Convert.ToChar(15).ToString(), "$O");
                ExtC39_Translation.Add(Convert.ToChar(16).ToString(), "$P");
                ExtC39_Translation.Add(Convert.ToChar(17).ToString(), "$Q");
                ExtC39_Translation.Add(Convert.ToChar(18).ToString(), "$R");
                ExtC39_Translation.Add(Convert.ToChar(19).ToString(), "$S");
                ExtC39_Translation.Add(Convert.ToChar(20).ToString(), "$T");
                ExtC39_Translation.Add(Convert.ToChar(21).ToString(), "$U");
                ExtC39_Translation.Add(Convert.ToChar(22).ToString(), "$V");
                ExtC39_Translation.Add(Convert.ToChar(23).ToString(), "$W");
                ExtC39_Translation.Add(Convert.ToChar(24).ToString(), "$X");
                ExtC39_Translation.Add(Convert.ToChar(25).ToString(), "$Y");
                ExtC39_Translation.Add(Convert.ToChar(26).ToString(), "$Z");
                ExtC39_Translation.Add(Convert.ToChar(27).ToString(), "%A");
                ExtC39_Translation.Add(Convert.ToChar(28).ToString(), "%B");
                ExtC39_Translation.Add(Convert.ToChar(29).ToString(), "%C");
                ExtC39_Translation.Add(Convert.ToChar(30).ToString(), "%D");
                ExtC39_Translation.Add(Convert.ToChar(31).ToString(), "%E");
                ExtC39_Translation.Add("!", "/A");
                ExtC39_Translation.Add("\"", "/B");
                ExtC39_Translation.Add("#", "/C");
                ExtC39_Translation.Add("$", "/D");
                ExtC39_Translation.Add("%", "/E");
                ExtC39_Translation.Add("&", "/F");
                ExtC39_Translation.Add("'", "/G");
                ExtC39_Translation.Add("(", "/H");
                ExtC39_Translation.Add(")", "/I");
                ExtC39_Translation.Add("*", "/J");
                ExtC39_Translation.Add("+", "/K");
                ExtC39_Translation.Add(",", "/L");
                ExtC39_Translation.Add("/", "/O");
                ExtC39_Translation.Add(":", "/Z");
                ExtC39_Translation.Add(";", "%F");
                ExtC39_Translation.Add("<", "%G");
                ExtC39_Translation.Add("=", "%H");
                ExtC39_Translation.Add(">", "%I");
                ExtC39_Translation.Add("?", "%J");
                ExtC39_Translation.Add("[", "%K");
                ExtC39_Translation.Add("\\", "%L");
                ExtC39_Translation.Add("]", "%M");
                ExtC39_Translation.Add("^", "%N");
                ExtC39_Translation.Add("_", "%O");
                ExtC39_Translation.Add("{", "%P");
                ExtC39_Translation.Add("|", "%Q");
                ExtC39_Translation.Add("}", "%R");
                ExtC39_Translation.Add("~", "%S");
                ExtC39_Translation.Add("`", "%W");
                ExtC39_Translation.Add("@", "%V");
                ExtC39_Translation.Add("a", "+A");
                ExtC39_Translation.Add("b", "+B");
                ExtC39_Translation.Add("c", "+C");
                ExtC39_Translation.Add("d", "+D");
                ExtC39_Translation.Add("e", "+E");
                ExtC39_Translation.Add("f", "+F");
                ExtC39_Translation.Add("g", "+G");
                ExtC39_Translation.Add("h", "+H");
                ExtC39_Translation.Add("i", "+I");
                ExtC39_Translation.Add("j", "+J");
                ExtC39_Translation.Add("k", "+K");
                ExtC39_Translation.Add("l", "+L");
                ExtC39_Translation.Add("m", "+M");
                ExtC39_Translation.Add("n", "+N");
                ExtC39_Translation.Add("o", "+O");
                ExtC39_Translation.Add("p", "+P");
                ExtC39_Translation.Add("q", "+Q");
                ExtC39_Translation.Add("r", "+R");
                ExtC39_Translation.Add("s", "+S");
                ExtC39_Translation.Add("t", "+T");
                ExtC39_Translation.Add("u", "+U");
                ExtC39_Translation.Add("v", "+V");
                ExtC39_Translation.Add("w", "+W");
                ExtC39_Translation.Add("x", "+X");
                ExtC39_Translation.Add("y", "+Y");
                ExtC39_Translation.Add("z", "+Z");
                ExtC39_Translation.Add(Convert.ToChar(127).ToString(), "%T"); //also %X, %Y, %Z 
            }
            private void InsertExtendedCharsIfNeeded(ref string FormattedData)
            {
                string output = "";
                foreach (char c in FormattedData)
                {
                    try
                    {
                        string s = C39_Code[c].ToString();
                        output += c;
                    }//try
                    catch
                    {
                        //insert extended substitution
                        object oTrans = ExtC39_Translation[c.ToString()];
                        output += oTrans.ToString();
                    }//catch
                }//foreach

                FormattedData = output;
            }
            private char getChecksumChar(string strNoAstr)
            {
                //checksum
                string Code39_Charset = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ-. $/+%";
                InsertExtendedCharsIfNeeded(ref strNoAstr);
                int sum = 0;

                //Calculate the checksum
                for (int i = 0; i < strNoAstr.Length; ++i)
                {
                    sum = sum + Code39_Charset.IndexOf(strNoAstr[i].ToString());
                }

                //return the checksum char
                return Code39_Charset[sum % 43];
            }
            #region IBarcode Members

            public string Encoded_Value
            {
                get { return Encode_Code39(); }
            }

            #endregion
        }//class
        #endregion
    }
}