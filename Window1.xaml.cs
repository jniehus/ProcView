using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Drawing;
using System.Windows.Media.Animation;
using System.Threading;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Xml;

namespace ProcView
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {

        private KernelCollection kernelCollection = new KernelCollection();
        private BackgroundWorker backgroundWorker = new BackgroundWorker();
        private Style _style, _errorStyle;
        private string _imagePath = string.Empty;
        private string _imageName = string.Empty;
        private string _kernelName = string.Empty;
        private string _customFilterXML = string.Empty;

        public Window1()
        {
            InitializeComponent();
            PopulatePresetFilterComboBox();

            //initialize image controls for both image displays
            ImageControls icImageDisplay = new ImageControls(userImage, userImageCanvas, processedImage, processedImageCanvas);

            //initialize backgroundworker's parameters
            backgroundWorker.WorkerReportsProgress = true;
            backgroundWorker.WorkerSupportsCancellation = true;
            backgroundWorker.DoWork += backgroundWorker_DoWork;
            backgroundWorker.ProgressChanged += backgroundWorker_ProgressChanged;
            backgroundWorker.RunWorkerCompleted += backgroundWorker_RunWorkerCompleted;

            //set _style for Dynamically added TextBoxes for the custom Grid control
            ResourceDictionary res = (ResourceDictionary)Application.LoadComponent(new Uri("ProcViewDictionary.xaml", UriKind.Relative));
            Style style = (Style)res["textBoxStlye"];
            Style errorStyle = (Style)res["textBoxErrorStlye"];
            _style = style;
            _errorStyle = errorStyle;

            //automatically load customkernel file if it exists
            string curDir = Directory.GetCurrentDirectory();
            string loadFile = curDir + "\\CustomKernels.xml";
            if (System.IO.File.Exists(loadFile))
            {
                LoadCustomFilterXML(loadFile);
            }

        }

        /// <summary>
        /// Populate preset filter combo box and set default value to Lowpass (smoothing filter)
        /// </summary>
        private void PopulatePresetFilterComboBox()
        {
            presetFilterCombo.Items.Add("Lowpass");
            presetFilterCombo.Items.Add("Weighted Lowpass");
            presetFilterCombo.Items.Add("Negative");
            presetFilterCombo.Items.Add("High Pass");
            presetFilterCombo.Items.Add("Normalized Sobel X");
            presetFilterCombo.Items.Add("Normalized Sobel Y");
            presetFilterCombo.SelectedItem = "Lowpass";
        }

        /// <summary>
        /// Basic File menu commands (Open image, close program, save processed image)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem mi = e.Source as MenuItem;
            switch (mi.Name)
            {
                case "OpenImage":
                    {
                        Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
                        dlg.Filter = "Image Files(*.BMP;*.JPG;*.PNG)|*.BMP;*.JPG;*.PNG"; // Filter files by extension
                        dlg.Title = "Open Image";
                        // Show open file dialog box.
                        Nullable<bool> result = dlg.ShowDialog();
                        if (result == true)
                        {
                            string openImage = dlg.FileName.ToString();
                            _imageName = openImage;
                            DisplayOpenedImage(openImage);
                        }
                        break;
                    }
                case "LoadCustomFilters":
                    {
                        customFilterRadio.IsChecked = true;
                        Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
                        dlg.Filter = "XML Files(*.XML)|*.XML"; // Filter files by extension
                        dlg.Title = "Load Custom Filters";
                        // Show open file dialog box.
                        Nullable<bool> result = dlg.ShowDialog();
                        if (result == true)
                        {
                            string openCustomFilterXML = dlg.FileName.ToString();
                            _customFilterXML = openCustomFilterXML;
                            LoadCustomFilterXML(openCustomFilterXML);
                        }
                        break;
                    }
                case "SaveAs":
                    {
                        if (processedImage.Source != null)
                        {
                            _imageName = System.IO.Path.GetFileName(_imagePath);
                            SaveProcessedImage(_kernelName + "_" + _imageName);
                        }
                        else
                        {
                            MessageBox.Show("Create a processed Image first");
                        }

                        break;
                    }
                case "Exit":
                    {
                        this.Close();
                        break;
                    }
            }
        }

        /// <summary>
        /// Displays the user's image they selected from the filesystem
        /// </summary>
        /// <param name="path">image from filesystem</param>
        public void DisplayOpenedImage(string path)
        {
            //Remove previous processed image 
            processedImage.Source = null;
            SaveAs.IsEnabled = false;
            //Display in GUI
            BitmapImage myBitMap = new BitmapImage();
            myBitMap.BeginInit();
            myBitMap.UriSource = new Uri(@path);
            myBitMap.EndInit();
            userImage.Source = myBitMap;
            _imagePath = path;
            statusBarImagePath.Text = "";
            statusBarImagePath.Text = path;
        }

        public void LoadCustomFilterXML(string customFilterXML)
        {

            FileStream fs = new FileStream(customFilterXML, FileMode.Open);
            XmlDictionaryReader reader = XmlDictionaryReader.CreateTextReader(fs, new XmlDictionaryReaderQuotas());
            DataContractSerializer ser = new DataContractSerializer(typeof(KernelCollection));
            // Deserialize the data and read it from the instance.
            KernelCollection deserializedList = new KernelCollection();
            deserializedList = (KernelCollection)ser.ReadObject(reader, true);
            kernelCollection.kernelList.Clear();
            kernelCollection.kernelList = deserializedList.kernelList;
            //deserialize the serilized matricies in the collection
            foreach (Kernel k in kernelCollection)
            {
                k.matrix = k.DeserializeMatrix(k.SerialMatrix);
            }

            PopulateCustomKernelDropBox(string.Empty);
            reader.Close();
            fs.Close();
        }

        /// <summary>
        /// Saves processed Image
        /// </summary>
        /// <param name="procImageName">Prepends original image name with type of filter used to generate image</param>
        public void SaveProcessedImage(string procImageName)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.Filter = "JPEG Image(*.JPG)|*.JPG"; // Can only images as JPG
            dlg.AddExtension = true;
            dlg.ValidateNames = true;
            dlg.FileName = procImageName;
            dlg.Title = "Save Processed Image"; 
            // Show save file dialog box
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                string procImagePath = dlg.FileName.ToString();
                FileStream stream = new FileStream(procImagePath, FileMode.Create);
                JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                TextBlock myTextBlock = new TextBlock();
                myTextBlock.Text = "Codec Author is: " + encoder.CodecInfo.Author.ToString();
                encoder.FlipHorizontal = false;
                encoder.FlipVertical = false;
                encoder.QualityLevel = 100;
                encoder.Frames.Add(BitmapFrame.Create((BitmapSource)processedImage.Source));
                encoder.Save(stream);
                stream.Close();

            }
        }

        //Radio button to select custom input.  This will disable preset filter controls
        //and enable the custom filter controls
        private void customFilterRadio_Checked(object sender, RoutedEventArgs e)
        {
            presetFilterCombo.IsEnabled = false;
            customKernelSizeSlider.IsEnabled = true;
            customKernelGrid.IsEnabled = true;
            randomCustomFilterButton.IsEnabled = true;
            saveCustomFilterButton.IsEnabled = true;
            customFilterComboBox.IsEnabled = true;
        }

        //Radio button to select preset filter (default).  While active, this disables
        //custom filter controls.
        private void presetFilterRadio_Checked(object sender, RoutedEventArgs e)
        {
            if (presetFilterCombo != null)
            {
                presetFilterCombo.IsEnabled = true;
            }
            if (customKernelSizeSlider != null)
            {
                customKernelSizeSlider.IsEnabled = false;
            }
            if (customKernelGrid != null)
            {
                customKernelGrid.IsEnabled = false;
            }
            if (randomCustomFilterButton != null)
            {
                randomCustomFilterButton.IsEnabled = false;
            }
            if (saveCustomFilterButton != null)
            {
                saveCustomFilterButton.IsEnabled = false;
            }
            if (customFilterComboBox != null)
            {
                customFilterComboBox.IsEnabled = false;
            }
        }

        //Filter selection to apply to image
        private void presetFilterCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string kernelType = presetFilterCombo.SelectedItem.ToString();
            Kernel selectedKernel = new Kernel(kernelType);
            int size = selectedKernel.matrixSize;
            PopulateUIKernelGrid(size, kernelType, presetKernelGrid, selectedKernel);
        }

        /// <summary>
        /// Displays the processed image
        /// </summary>
        /// <param name="img">processed image</param>
        public void DisplayFilteredImage(Bitmap img)
        {
            SaveAs.IsEnabled = true;
            MemoryStream ms = new MemoryStream();
            img.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
            BitmapImage bImg = new BitmapImage();
            bImg.BeginInit();
            bImg.StreamSource = new MemoryStream(ms.ToArray());
            bImg.EndInit();   
            processedImage.Source = bImg;
            ms.Close();
            ProgressBarControl.Clear(progbar, progbarLabel);
        }

        /// <summary>
        /// Updates UI kernel grids to display kernel contents.
        /// </summary>
        /// <param name="size">size of kernel matrix</param>
        /// <param name="type">preset, custom, or random</param>
        /// <param name="uiGrid">grid to display kernel in</param>
        /// <param name="uiKernel">kernel to be displayed</param>
        private void PopulateUIKernelGrid(int size, string type, Grid uiGrid, Kernel uiKernel)
        {

            ResourceDictionary res = (ResourceDictionary)Application.LoadComponent(new Uri("ProcViewDictionary.xaml", UriKind.Relative));
            Style style = (Style)res["textBoxStlye"];

            //leftover pixels for the kernel grid display. Need to distribute the xtra pixels among the columns
            int xtra = 160 % size;
            int control = 0;

            uiGrid.Children.Clear();
            uiGrid.RowDefinitions.Clear();
            uiGrid.ColumnDefinitions.Clear();
            for (int x = 0; x < size; x++)
            {
                RowDefinition row = new RowDefinition();
                uiGrid.RowDefinitions.Add(row);
                for (int y = 0; y < size; y++)
                {
                    TextBox myTxtBox = new TextBox();
                    myTxtBox.Style = style;
                    myTxtBox.ToolTip = "Please enter an integer between -255 and 255";
                    if (type == "userCustom")
                    {
                        myTxtBox.IsEnabled = true;
                    }
                    else if (type == "randomCustom")
                    {
                        myTxtBox.IsEnabled = true;
                        myTxtBox.Text = uiKernel.matrix[x, y].ToString();
                    }
                    else
                    {
                        myTxtBox.IsEnabled = false;
                        myTxtBox.Text = uiKernel.matrix[x, y].ToString();
                    }
                    ColumnDefinition col = new ColumnDefinition();
                    if (control < xtra && xtra > 0)
                    {
                        col.Width = new GridLength((160 / size) + 1);
                        control += 1;
                    }
                    else
                    {
                        col.Width = new GridLength(160 / size);
                    }
                    uiGrid.ColumnDefinitions.Add(col);
                    uiGrid.Children.Add(myTxtBox);
                    Grid.SetRow(myTxtBox, y);
                    Grid.SetColumn(myTxtBox, x);
                }
            }            
        }

        //Select the size of custom kernel. Constraint: max up to 7
        private void customKernelSizeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int size = (int)e.NewValue;
            Kernel customKernel = new Kernel("userCustom");
            PopulateUIKernelGrid(size, "userCustom", customKernelGrid, customKernel);
        }

        //creates a kernel with random numbers between -10 and 10 and updates UIKernelGrid
        private void randomCustomFilterButton_Click(object sender, RoutedEventArgs e)
        {
            Random rnd = new Random();
            double sliderRandom = Math.Floor(rnd.NextDouble() * 10); 
            if (sliderRandom < 3)
            {
                customKernelSizeSlider.Value = 3;
            }
            else if (sliderRandom >= 3 && sliderRandom < 7)
            {
                customKernelSizeSlider.Value = 5;
            }
            else
            {
                customKernelSizeSlider.Value = 7;
            }

            int size = (int)customKernelSizeSlider.Value;
            int[,] rndKernel = new int[size, size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int negV = -1;
                    double chkRnd = rnd.NextDouble() * 10;
                    if (chkRnd < 8)
                    {
                        negV = 1;
                    }
                    int element = (int)Math.Floor((rnd.NextDouble() * 10)*negV);
                    rndKernel[x, y] = element;
                }
            }

            Kernel randKernel = new Kernel(rndKernel);
            PopulateUIKernelGrid(size, "randomCustom", customKernelGrid, randKernel);
        }

        //Starts algorithm to process image
        private void applyKernelButton_Click(object sender, RoutedEventArgs e)
        {

            Kernel selectedKernel;
            
            if (customFilterRadio.IsChecked == true && userImage.Source != null)
            {
                bool valCustFilter = ValidateCustomFilter();
                if (valCustFilter == true)
                {
                    selectedKernel = new Kernel(GrabCustomFilter());
                    _kernelName = "CustomFilter";
                    if (_imagePath != string.Empty)
                    {
                        InvokeFilterProcess(selectedKernel);
                    }
                }
            }
            else if (presetFilterRadio.IsChecked == true && userImage.Source != null)
            {
                if (presetFilterCombo.SelectedItem != null)
                {
                    string kernelType = presetFilterCombo.SelectedItem.ToString();
                    _kernelName = kernelType;
                    selectedKernel = new Kernel(kernelType);
                    if (_imagePath != string.Empty)
                    {
                        InvokeFilterProcess(selectedKernel);
                    }
                }
                else
                {
                    MessageBox.Show("Please select a filter from the dropdown box.");
                }
            }
            else
            {
                MessageBox.Show("Please open an image");
                ProgressBarControl.Clear(progbar, progbarLabel);
            }
        }

        //Verifies the values in the custom Kernel UI
        private bool ValidateCustomFilter()
        {
            BrushConverter conv = new BrushConverter();
            int gridSize = (int)customKernelSizeSlider.Value;
            int i = 0;
            bool valid = true;
            for (int y = 0; y < gridSize; y++)
            {
                i = y * gridSize;
                for (int x = 0; x < gridSize; x++)
                {
                    TextBox test = customKernelGrid.Children[i] as TextBox;
                    string svalue = test.Text;
                    try
                    {
                        int.Parse(svalue);
                        test.Style = _style;
                    }
                    catch
                    {
                        
                        valid = false;
                        test.Style = _errorStyle;
                    }

                    i += 1;
                }
            }

            if (valid == false)
            {
                MessageBox.Show("Please enter an integer between -255 and 255.");
            }
            ProgressBarControl.Clear(progbar, progbarLabel);
            return valid;
        }

        /// <summary>
        /// Returns the values stored in the custom kernel UI
        /// </summary>
        /// <returns></returns>
        private int[,] GrabCustomFilter()
        {
            int gridSize = (int)customKernelSizeSlider.Value;
            int i = 0;
            int value;
            int[,] userMatrix = new int[gridSize, gridSize];
            for (int y = 0; y < gridSize; y++)
            {
                i = y * gridSize;
                for (int x = 0; x < gridSize; x++)
                {
                    TextBox test = customKernelGrid.Children[i] as TextBox;
                    string svalue = test.Text;
                    value = int.Parse(svalue);
                    userMatrix[y, x] = value;
                    i += 1;
                }
            }

            return userMatrix;
        }

        /// <summary>
        /// Starts thread to begin filter process
        /// </summary>
        /// <param name="selectedKernel">kernel to use on image</param>
        /// <param name="proc">processimage object</param>
        private void InvokeFilterProcess(Kernel selectedKernel)
        {
            //Hide apply filter button so we dont launch multiple threads on same object
            applyKernelButton.IsEnabled = false;
            //Show Progressbar
            ProgressBarControl.Start(progbar, progbarLabel);
            Bitmap img = new Bitmap(_imagePath);
            ImageProcessInput input = new ImageProcessInput(img, selectedKernel);
            backgroundWorker.RunWorkerAsync(input);
        }

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            // Get the input values.
            ImageProcessInput input = (ImageProcessInput)e.Argument;

            //create ImageProcess object
            ImageProcess proc = new ImageProcess();
            Bitmap pImg = new Bitmap(input.Img.Width, input.Img.Height);

            // Start the search for primes and wait.
            pImg = proc.FilterImage(input.Img, input.Kernel, backgroundWorker);

            // Return the result.
            e.Result = pImg;
        }

        private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                // An error was thrown by the DoWork event handler.
                MessageBox.Show(e.Error.Message);
            }
            else
            {
                Bitmap img = (Bitmap)e.Result;
                DisplayFilteredImage(img);
            }

            applyKernelButton.IsEnabled = true;
            ProgressBarControl.Clear(progbar, progbarLabel);
        }

        private void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progbar.Value = (double)e.ProgressPercentage;
        }

        /// <summary>
        /// Verifies the values in the custom kernel UI
        /// </summary>
        /// <param name="valid">true or false</param>
        private void HandleValidation(bool valid)
        {
            if (valid == false)
            {
                MessageBox.Show("Please enter an integer between -255 and 255.");
            }
        }

        private void saveCustomFilterButton_Click(object sender, RoutedEventArgs e)
        {

            bool valCustFilter = ValidateCustomFilter();
            if (valCustFilter == true)
            {
                Kernel customKernel = new Kernel(GrabCustomFilter());
                KernelNameDialog kName = new KernelNameDialog();
                kName.ShowDialog();
                customKernel.Name = kName.customKernelName;
                kernelCollection.AddKernel(customKernel);
                PopulateCustomKernelDropBox(customKernel.Name);

                string curDir = Directory.GetCurrentDirectory();
                string saveFilterCollection = curDir + "\\CustomKernels.xml";
                WriteObject(saveFilterCollection);

            }
        }

        public void WriteObject(string path)
        {
            // Create a new instance of a StreamWriter
            // to read and write the data.
            FileStream fs = new FileStream(path, FileMode.Create);
            XmlDictionaryWriter writer = XmlDictionaryWriter.CreateTextWriter(fs);
            DataContractSerializer ser = new DataContractSerializer(typeof(KernelCollection));
            ser.WriteObject(writer, kernelCollection);
            writer.Close();
            fs.Close();
        }

        private void PopulateCustomKernelDropBox(string customKernelName)
        {
            //cleare the customcombobox
            customFilterComboBox.Items.Clear();
            foreach (Kernel k in kernelCollection)
            {
                customFilterComboBox.Items.Add(k.Name);
                if (customKernelName != string.Empty)
                {
                    customFilterComboBox.SelectedItem = customKernelName;
                }
                else
                {
                    customFilterComboBox.SelectedIndex = 0;
                }
            }
        }

        private void customFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int kernelIndex = customFilterComboBox.SelectedIndex;
            if (kernelIndex >= 0)
            {
                Kernel iKernel = new Kernel(kernelCollection.kernelList[kernelIndex].matrix);
                int size = iKernel.matrixSize;
                PopulateUIKernelGrid(size, "randomCustom", customKernelGrid, iKernel);
            }
        }
    }
}
