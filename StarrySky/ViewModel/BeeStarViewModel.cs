using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StarrySky.View;
using StarrySky.Model;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using DispatcherTimer = Windows.UI.Xaml.DispatcherTimer;
using UIElement = Windows.UI.Xaml.UIElement;
using Windows.Foundation;

namespace StarrySky.ViewModel
{
    class BeeStarViewModel
    {
        /// <summary>
        /// Sprites collection.
        /// </summary>
        private readonly ObservableCollection<UIElement> _sprites =
            new ObservableCollection<UIElement>();

        /// <summary>
        /// TO DO
        /// </summary>
        public INotifyCollectionChanged Sprites { get { return _sprites; } }

        private readonly Dictionary<Star, StarControl> _stars = new Dictionary<Star, StarControl>();
        private readonly List<StarControl> _fadedStars = new List<StarControl>();

        private BeeStarModel _model = new BeeStarModel();

        private readonly Dictionary<Bee, AnimatedImage> _bees = new Dictionary<Bee, AnimatedImage>();

        private DispatcherTimer _timer = new DispatcherTimer();

        public Size PlayAreaSize
        {
            get { return _model.PlayAreaSize; }
            set { _model.PlayAreaSize = value; }
        }

        public BeeStarViewModel()
        {
            _model.BeeMoved += _model_BeeMoved;
            _model.StarChanged += _model_StarChanged;
            _timer.Interval = TimeSpan.FromSeconds(2);
            _timer.Tick += _timer_Tick;
            _timer.Start();
        }

        private void _timer_Tick(object sender, object e)
        {
            foreach (StarControl fadedStar in _fadedStars)
            {
                _sprites.Remove(fadedStar);
            }
            _model.Update(); 
        }
        private void _model_StarChanged(object sender, StarChangedEventArgs e)
        {
            Star starThatChanged = e.StarThatChanged;
            if (e.Removed)
            {
                StarControl starControl = _stars[starThatChanged];
                _fadedStars.Add(starControl);
                _stars.Remove(starThatChanged);
                starControl.FadeOut();
            }
            else
            {
                if(_stars.ContainsKey(starThatChanged))
                {
                    StarControl starControl;
                    _stars.TryGetValue(starThatChanged,out starControl);
                    if(starControl == null)
                    {
                        starControl = new StarControl();
                        BeeStarHelper.SendToBack(starControl);
                        starControl.FadeIn();
                        _sprites.Add(starControl);
                        _stars[starThatChanged] = starControl;
                        BeeStarHelper.SetCanvasLocation(starControl,
                            starThatChanged.Location.X, starThatChanged.Location.Y);
                    }
                }
            }
        }

        private void _model_BeeMoved(object sender, BeeMovedEventArgs e)
        {
            Bee beeThatMoved = e.BeeThatMoved;
            if (_bees.ContainsKey(beeThatMoved))
            {
                AnimatedImage beeAnimatedImage = BeeStarHelper.BeeFactory(beeThatMoved.Width, beeThatMoved.Height,
                    TimeSpan.FromMilliseconds(50));
                BeeStarHelper.SetCanvasLocation(beeAnimatedImage, e.X, e.Y);

                _bees[beeThatMoved] = beeAnimatedImage;
                _sprites.Add(beeAnimatedImage);
            }
            else
            {
                BeeStarHelper.MoveElementOnCanvas(_bees[beeThatMoved], 
                    e.X, e.Y);
            }

        }
    }
}
