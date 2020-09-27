using Richasy.Controls.Reader.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace Richasy.Controls.Reader.Views
{
    public partial class TxtView
    {
        public event EventHandler PrevPageSelected;
        public event EventHandler NextPageSelected;
        public event EventHandler<int> SelectionChanged;
        public event EventHandler<bool> LoadingChanged;
        public event EventHandler<PositionEventArgs> TouchHolding;
        public event EventHandler<PositionEventArgs> TouchTapped;

        private void OnPrevPageSelected()
        {
            PrevPageSelected?.Invoke(this, EventArgs.Empty);
        }

        private void OnNextPageSelected()
        {
            NextPageSelected?.Invoke(this, EventArgs.Empty);
        }

        private void OnSelectionChanged()
        {
            SelectionChanged?.Invoke(this, _startTextIndex);
        }

        private void TxtView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SizeChangeHandle();
        }

        public void SizeChangeHandle()
        {
            Debug.WriteLine(ParentWidth);
            _columns = Convert.ToInt32(Math.Ceiling(ParentWidth / SingleColumnMaxWidth));
            CreateContentDelayer.Delay();
        }

        private void CreateContentWaiter_Arrived(object sender, EventArgs e)
        {
            if(!_isSizeChangeLoaded)
            {
                _isSizeChangeLoaded = true;
                return;
            }
            else
            {
                CreateContent();
                IsCoreSelectedChanged = true;
                if (Index > Count - 1)
                {
                    Index = Convert.ToInt32(Math.Floor((Count - 1) / 2.0));
                }
                GoToIndex(Index, false);
                IsCoreSelectedChanged = false;
            }
        }
    }
}
