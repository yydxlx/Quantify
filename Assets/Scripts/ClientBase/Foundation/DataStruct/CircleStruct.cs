namespace ClientBase
{
    /// <summary>
    /// Circle structure to save data.
    /// </summary>
    /// <typeparam name="Data"></typeparam>
	public class CircleStruct<Data> where Data : class
    {

        private InternalData<Data>[] _list;
        private InternalData<Data> _prepareToSetData;

        /// <summary>
        /// Constructur to set array max count.
        /// </summary>
        /// <param name="maxCount"></param>
		public CircleStruct(int maxCount)
        {
            Assert.AssertAtLeast(maxCount, 1);
            _list = new InternalData<Data>[maxCount];
            for (int i = 0; i < _list.Length; i++)
            {
                _list[i] = new InternalData<Data>();
            }

            for (int i = 0; i < _list.Length; i++)
            {
                int nextIndex = (i + 1) % _list.Length;
                int preIndex = (i - 1) < 0 ? ((i - 1 + _list.Length) % _list.Length) : (i - 1);
                _list[i].Preview = _list[preIndex];
                _list[i].Next = _list[nextIndex];
            }

            _prepareToSetData = _list[0];
        }

        public Data[] GetListFromLast()
        {
            // Calculate count.
            int count = 0;
            var current = _prepareToSetData.Preview;
            for (int i = 0; i < _list.Length; i++)
            {
                if (current.MyData == null)
                    break;
                else
                {
                    current = current.Preview;
                    count++;
                }
            }

            Data[] result = new Data[count];
            current = _prepareToSetData.Preview;
            for (int i = 0; i < _list.Length; i++)
            {
                if (current.MyData == null)
                    break;
                else
                {
                    result[i] = current.MyData;
                    current = current.Preview;
                }
            }
            return result;
        }

        public void PushNewAnnouncement(Data _data)
        {
            _prepareToSetData.MyData = _data;
            _prepareToSetData = _prepareToSetData.Next;
        }

        private class InternalData<DataClass> where DataClass : class
        {
            public InternalData<DataClass> Preview = null;
            public Data MyData = null;
            public InternalData<DataClass> Next = null;
        }
    }
}