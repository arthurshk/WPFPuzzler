using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Input;
using System.Windows.Media;

namespace puzzler
{
    public partial class MainWindow : Window
    {
        private BitmapImage uploadedImage;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void UploadPicture_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.png;*.jpg)|*.png;*.jpg"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                uploadedImage = new BitmapImage(new Uri(openFileDialog.FileName));
                CreatePuzzle(uploadedImage);
            }
        }

        private void CreatePuzzle(BitmapImage image)
        {
            int rows = 4; 
            int cols = 4; 
            double pieceWidth = image.PixelWidth / cols;
            double pieceHeight = image.PixelHeight / rows;

            PiecesPanel.Children.Clear();
            PuzzleArea.Children.Clear();

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    CroppedBitmap piece = new CroppedBitmap(image, new Int32Rect((int)(col * pieceWidth), (int)(row * pieceHeight), (int)pieceWidth, (int)pieceHeight));
                    Image pieceImage = new Image
                    {
                        Source = piece,
                        Width = pieceWidth,
                        Height = pieceHeight,
                        Tag = new Point(col, row) 
                    };

                    pieceImage.MouseDown += Piece_MouseDown;
                    PiecesPanel.Children.Add(pieceImage);
                }
            }
        }

        private void Piece_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Image piece = sender as Image;
            DataObject data = new DataObject(piece);
            DragDrop.DoDragDrop(piece, data, DragDropEffects.Move);
        }

        private void PuzzleArea_Drop(object sender, DragEventArgs e)
        {
            Image piece = e.Data.GetData(typeof(Image)) as Image;
            if (piece != null)
            {
                Point dropPosition = e.GetPosition(PuzzleArea);
                Canvas.SetLeft(piece, dropPosition.X - piece.Width / 2);
                Canvas.SetTop(piece, dropPosition.Y - piece.Height / 2);
                PuzzleArea.Children.Add(piece);
                PiecesPanel.Children.Remove(piece);

                CheckPuzzleSolved();
            }
        }

        private void CheckPuzzleSolved()
        {
            foreach (UIElement child in PuzzleArea.Children)
            {
                if (child is Image piece)
                {
                    Point originalPosition = (Point)piece.Tag;
                    Point currentPosition = new Point(Canvas.GetLeft(piece), Canvas.GetTop(piece));

                    double correctX = originalPosition.X * piece.Width;
                    double correctY = originalPosition.Y * piece.Height;

                    if (Math.Abs(correctX - currentPosition.X) > piece.Width / 10 || Math.Abs(correctY - currentPosition.Y) > piece.Height / 10)
                    {
                        return; 
                    }
                }
            }

            MessageBox.Show("Puzzle Solved!");
        }
    }
}
