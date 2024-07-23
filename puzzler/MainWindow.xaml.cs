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
        private Point _startPoint;
        private Image _draggedPiece;
        private bool _isDragging;

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
            double scale = 0.5;
            TransformedBitmap scaledBitmap = new TransformedBitmap(bitmap, new ScaleTransform(scale, scale));

            int rows = 4;
            int cols = 4;
            double pieceWidth = scaledBitmap.PixelWidth / cols;
            double pieceHeight = scaledBitmap.PixelHeight / rows;

            PiecesPanel.Children.Clear();

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    CroppedBitmap croppedBitmap = new CroppedBitmap(scaledBitmap, new Int32Rect(j * (int)pieceWidth, i * (int)pieceHeight, (int)pieceWidth, (int)pieceHeight));
                    Image piece = new Image
                    {
                        Width = pieceWidth,
                        Height = pieceHeight,
                        Source = croppedBitmap,
                        Tag = new Point(j, i),
                        Margin = new Thickness(5)
                    };
                    piece.MouseLeftButtonDown += Piece_MouseLeftButtonDown;
                    piece.MouseMove += Piece_MouseMove;
                    piece.MouseLeftButtonUp += Piece_MouseLeftButtonUp;
                    PiecesPanel.Children.Add(piece);
                }
            }
        }

        private void Piece_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _draggedPiece = sender as Image;
            if (_draggedPiece != null)
            {
                _startPoint = e.GetPosition(PuzzleArea);
                _isDragging = true;
                _draggedPiece.CaptureMouse();
            }
        }

        private void Piece_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging && _draggedPiece != null)
            {
                Point currentPosition = e.GetPosition(PuzzleArea);
                double offsetX = currentPosition.X - _startPoint.X;
                double offsetY = currentPosition.Y - _startPoint.Y;

                Canvas.SetLeft(_draggedPiece, Canvas.GetLeft(_draggedPiece) + offsetX);
                Canvas.SetTop(_draggedPiece, Canvas.GetTop(_draggedPiece) + offsetY);

                _startPoint = currentPosition;
            }
        }

        private void Piece_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDragging && _draggedPiece != null)
            {
                _draggedPiece.ReleaseMouseCapture();
                _isDragging = false;

                Point dropPosition = e.GetPosition(PuzzleArea);
                Canvas.SetLeft(_draggedPiece, dropPosition.X - _draggedPiece.Width / 2);
                Canvas.SetTop(_draggedPiece, dropPosition.Y - _draggedPiece.Height / 2);

                if (_draggedPiece.Parent is Panel oldParent)
                {
                    oldParent.Children.Remove(_draggedPiece);
                }

                PuzzleArea.Children.Add(_draggedPiece);
                CheckPuzzleSolved();
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

                    double correctX = originalPosition.X * piece.Width + piece.Width / 2;
                    double correctY = originalPosition.Y * piece.Height + piece.Height / 2;

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
