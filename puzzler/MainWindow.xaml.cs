using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace puzzler
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void UploadPicture_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "Image Files (*.png; *.jpg)|*.png;*.jpg";

            bool? result = dlg.ShowDialog();

            if (result == true)
            {
                string filename = dlg.FileName;
                BitmapImage bitmap = new BitmapImage(new Uri(filename));
                LoadPuzzlePieces(bitmap);
            }
        }

        private void LoadPuzzlePieces(BitmapImage bitmap)
        {
            int rows = 4;
            int cols = 4;
            double pieceWidth = bitmap.PixelWidth / cols;
            double pieceHeight = bitmap.PixelHeight / rows;

            PiecesPanel.Children.Clear();

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    CroppedBitmap croppedBitmap = new CroppedBitmap(bitmap, new Int32Rect(j * (int)pieceWidth, i * (int)pieceHeight, (int)pieceWidth, (int)pieceHeight));
                    Image piece = new Image
                    {
                        Width = pieceWidth,
                        Height = pieceHeight,
                        Source = croppedBitmap,
                        Tag = new Point(j, i),
                        Margin = new Thickness(5)
                    };
                    piece.MouseDown += Piece_MouseDown;
                    PiecesPanel.Children.Add(piece);
                }
            }
        }

        private void Piece_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Image piece = sender as Image;
            if (piece != null)
            {
                DataObject data = new DataObject(typeof(Image), piece);
                DragDrop.DoDragDrop(piece, data, DragDropEffects.Move);
            }
        }

        private void PuzzleArea_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(Image)))
            {
                Image piece = e.Data.GetData(typeof(Image)) as Image;
                if (piece != null)
                {
                    if (piece.Parent is Panel oldParent)
                    {
                        oldParent.Children.Remove(piece);
                    }
                    Point dropPosition = e.GetPosition(PuzzleArea);
                    Canvas.SetLeft(piece, dropPosition.X - piece.Width / 2);
                    Canvas.SetTop(piece, dropPosition.Y - piece.Height / 2);
                    PuzzleArea.Children.Add(piece);

                    CheckPuzzleSolved();
                }
            }
        }

        private void CheckPuzzleSolved()
        {
            bool puzzleSolved = true;

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
                        puzzleSolved = false;
                        break;
                    }
                }
            }

            if (puzzleSolved)
            {
                MessageBox.Show("Puzzle Solved!");
            }
        }
    }
}
