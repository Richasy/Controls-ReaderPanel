using Richasy.Controls.Reader.Enums;
using Richasy.Controls.Reader.Models;
using System;
using Windows.UI.Xaml;

namespace Richasy.Controls.Reader.Views
{
    public partial class ReaderViewBase
    {
        public event EventHandler PrevPageSelected;
        public event EventHandler NextPageSelected;
        public event EventHandler<int> ProgressChanged;
        public event EventHandler<LoadingStatus> LoadingStatusChanged;
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

        private void OnProgressChanged()
        {
            ProgressChanged?.Invoke(this, _startTextIndex);
        }

        private void ReaderViewBase_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (Visibility == Visibility.Visible)
                SizeChangeHandle();
        }

        public void SizeChangeHandle()
        {
            _columns = Convert.ToInt32(Math.Ceiling(ParentWidth / SingleColumnMaxWidth));
            if (!_isSetContent)
                CreateContentDelayer.Delay();
        }

        private async void CreateContentWaiter_Arrived(object sender, EventArgs e)
        {
            await CreateContent();
            IsCoreSelectedChanged = true;
            if (Index > Count - 1)
            {
                Index = Convert.ToInt32(Math.Floor((Count - 1) / 2.0));
            }
            GoToIndex(Index, false);
            IsCoreSelectedChanged = false;
            _isSizeChangeLoaded = true;
        }


    }
}
