using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace StarrySky.Model
{
    class BeeStarModel
    {
        #region Events
        public event EventHandler<BeeMovedEventArgs> BeeMoved;

        private void OnBeeMoved(BeeMovedEventArgs e)
        {
            var beeMovedEventHandler = BeeMoved;
            if (beeMovedEventHandler != null)
            {
                beeMovedEventHandler(this, e);
            }
        }

        public event EventHandler<StarChangedEventArgs> StarChanged;

        private void OnStarChanged(StarChangedEventArgs e)
        {
            var starChangedEventHandler = StarChanged;
            if (starChangedEventHandler != null)
                starChangedEventHandler(this, e);
        }
        #endregion

        /// <summary>
        ///  Star size
        /// </summary>
        public static readonly Size StarSize = new Size(150, 100);

        /// <summary>
        /// Group of bees
        /// </summary>
        private readonly Dictionary<Bee, Point> _bees = new Dictionary<Bee, Point>();

        /// <summary>
        /// Collection of stars
        /// </summary>
        private readonly Dictionary<Star, Point> _stars = new Dictionary<Star, Point>();

        private Random _random = new Random();

        /// <summary>
        /// Play area size
        /// </summary>
        private Size _playAreaSize;

        public BeeStarModel()
        {
            _playAreaSize = Size.Empty;
        }

        /// <summary>
        /// Updates state of application. 
        /// </summary>
        public void Update()
        {
            MoveOneBee();
            AddOrRemoveStar();
        }

        /// <summary>
        /// Checks if two rectangles overlapping 
        /// </summary>
        /// <param name="r1">First rectangle</param>
        /// <param name="r2">Second rectangle</param>
        /// <returns>True if rectangles overlapping</returns>
        private static bool RectsOverlap(Rect r1, Rect r2)
        {
            r1.Intersect(r2); // Saves intersection rectangle to r1
            if (r1.Width > 0 || r1.Height > 0)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Play area size.
        /// </summary>
        public Size PlayAreaSize
        {
            get { return _playAreaSize; }
            set
            {
                _playAreaSize = value;
                CreateBees();
                CreateStars();
            }
        }

        /// <summary>
        /// Create from 5 to 15 bees if there isn't any on the play area. If bees are on the play area they 
        /// will be moved to new locations.
        /// </summary>
        private void CreateBees()
        {
            if (PlayAreaSize.IsEmpty) // Check if area is empty
                return;
            else
            {
                if (_bees.Count > 0) // Check if there is any bee
                    foreach (Bee bee in _bees.Keys)
                    {
                        MoveOneBee(bee);
                    }
                else
                {
                    int beesAmount = _random.Next(5, 16);
                    Bee newBee = null;

                    for (int i = 0; i < beesAmount; i++)
                    {
                        var beeSize = new Size(_random.Next(40, 151), _random.Next(40, 151));
                        var beeLocation = FindNonOverlappingPoint(beeSize);
                        newBee = new Bee(beeLocation, beeSize);

                        _bees.Add(newBee,beeLocation);
                        OnBeeMoved(new BeeMovedEventArgs(newBee, beeLocation.X, beeLocation.Y));                 
                    }
                }
            }
        }

        /// <summary>
        /// Creates from 5 to 10 stars if there isn't any on the play area. If stars exist they 
        /// will be moved to new locations.
        /// </summary>
        private void CreateStars()
        {
            if (PlayAreaSize.IsEmpty) // Check if area is empty
                return;
            else
            {
                if (_stars.Count > 0) // Check if there is any bee
                    foreach (Star star in _stars.Keys)
                    {
                        star.Location = FindNonOverlappingPoint(StarSize);
                        OnStarChanged(new StarChangedEventArgs(star, false));
                    }
                else
                {
                    int starsAmount = _random.Next(5, 11);
                    for (int i = 0; i < starsAmount; i++)
                    {
                        CreateAStar();
                    }
                }
            }
        }

        /// <summary>
        /// Creates one star.
        /// </summary>
        private void CreateAStar()
        {
            Point newStarLocation = FindNonOverlappingPoint(StarSize);
            Star newStar = new Star(newStarLocation);
            _stars.Add(newStar, newStarLocation);
            OnStarChanged(new StarChangedEventArgs(newStar, false));
        }

        /// <summary>
        /// Finds non overlapping point on play area if there is enough free place.  
        /// </summary>
        /// <param name="size"> Tested control (rectangle) size.</param>
        /// <returns> Non overlapping point if there is free place on play area. Any point on play area otherwise.</returns>
        private Point FindNonOverlappingPoint(Size size)
        {
            if (PlayAreaSize.IsEmpty)
                throw new NullReferenceException("Property PlayAreaSize is empty.");

            int loopsCounter = 0;
            while (true)
            {
                // Create new random point in the play area
                Point rectLeftTopCorner = new Point(
                    _random.Next((int)PlayAreaSize.Width-150), _random.Next((int)PlayAreaSize.Height)-150);
                // With this point and given size create rectangle
                Rect testRectangle = new Rect(rectLeftTopCorner, size);

                var overlappingBees =
                    from bee in _bees.Keys
                    where RectsOverlap(testRectangle, bee.Position)
                    select bee;

                var overlappingStars =
                    from star in _stars.Keys
                    where RectsOverlap(testRectangle, new Rect(star.Location, StarSize))
                    select star;

                if ((overlappingBees.Count() + overlappingStars.Count()) == 0||loopsCounter > 1000) // Limit amount of test loops
                    return rectLeftTopCorner;
                loopsCounter++;
            }
        }

        /// <summary>
        /// Moves one bee at the free place. 
        /// </summary>
        /// <param name="bee">Bee to move. If equals null random bee will be moved.</param>
        private void MoveOneBee(Bee bee = null)
        {
            if (!_bees.Any())
                return;
            else
            {
                if (bee == null)
                    bee =_bees.Keys.ToList()[_random.Next(_bees.Count)]; // Select random bee

                Point newLocation = FindNonOverlappingPoint(bee.Size);
                bee.Location = newLocation; // Actualize bee object
                _bees[bee] = newLocation; // Actualize bee dictionary
                OnBeeMoved(new BeeMovedEventArgs(bee, newLocation.X, newLocation.Y));
            }
        }

        /// <summary>
        /// Randomly adds or removes star if stars amount is inside (5,20) interval. 
        /// If stars amount is outside interval this is no more random decision.
        /// </summary>
        private void AddOrRemoveStar()
        {
            int starsAmount = _stars.Count;
            if (starsAmount <= 5)
                CreateAStar();
            else if (starsAmount >= 20)
            {
                RemoveRandomStar();
            }
            else
            {
                if (_random.Next(2) == 0)
                    CreateAStar();
                else
                    RemoveRandomStar();
            }
        }

        /// <summary>
        /// Removes one random star.
        /// </summary>
        private void RemoveRandomStar()
        {
            Star randomStar = _stars.Keys.ToList()[_random.Next(_stars.Count)];
            _stars.Remove(randomStar);
            OnStarChanged(new StarChangedEventArgs(randomStar, true));
        }
    }
}
