using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;

namespace KinectV2_Fingerspelling.Controllers
{

    /// <summary>
    /// Data container
    /// </summary>
    public class DataContainer
    {
        #region Members
        /// <summary>
        /// Recorded color frames 
        /// </summary>
        private List<byte[]> listColorFrames = new List<byte[]>();

        /// <summary>
        /// Recorded depth frames 
        /// </summary>
        private List<ushort[]> listDepthFrames = new List<ushort[]>();

        /// <summary>
        /// Recorded body index frames 
        /// </summary>
        private List<byte[]> listBodyIndexFrames = new List<byte[]>();

        /// <summary>
        /// Body joints tracked over time
        /// </summary>
        private List<IList<Body>> listBodies = new List<IList<Body>>();

        /// <summary>
        /// Facedata detected overt time
        /// </summary>
        private List<FaceData> listFaceData = new List<FaceData>();

        #endregion

        #region Public Methods
        
        /// <summary>
        /// Get the list of all color frames
        /// </summary>
        public List<byte[]> AllColor
        {
            get { return this.listColorFrames; }
        }

        /// <summary>
        /// Add a single color frame
        /// </summary>
        public byte[] AddColor
        {
            set { this.listColorFrames.Add(value); }
        }

        public List<ushort[]> AllDepth
        {
            get { return this.listDepthFrames; }
        }

        public ushort[] AddDepth
        {
            set { this.listDepthFrames.Add(value); }
        }

        public List<byte[]> AllBodyIndex
        {
            get { return this.listBodyIndexFrames; }
        }

        public byte[] AddBodyIndex
        {
            set { this.listBodyIndexFrames.Add(value); }
        }

        public List<IList<Body>> AllListOfBodies
        {
            get { return this.listBodies; }
        }

        public IList<Body> AddListOfBodies
        {
            set { this.listBodies.Add(value); }
        }

        public List<FaceData> AllFaceData
        {
            get { return this.listFaceData; }
        }

        public FaceData AddFaceData
        {
            set { this.listFaceData.Add(value); }
        }
        
    

        /// <summary>
        /// Free the memory occupied by the data container
        /// </summary>
        public void ClearData()
        {
            this.listColorFrames.Clear();
            this.listDepthFrames.Clear();
            this.listBodyIndexFrames.Clear();
            this.listBodies.Clear();
            this.listFaceData.Clear();
            GC.Collect();
        }

        /// <summary>
        /// Class destructor
        /// </summary>
        ~DataContainer()
        {
            this.ClearData();           
        }

        #endregion
    }

}

