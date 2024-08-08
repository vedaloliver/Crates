
<script>
import axios from 'axios';

export default {
  name: 'ImageAnalysis',
  data() {
    return {
      caption: '',
      captionConfidence: 0,
      textBlocks: [],
    };
  },
  props: {
    imageUrl: {
      type: String,
      required: true
    }
  },
  methods: {
    async analyzeImage(imageUrlToAnalyze) {
      if (!imageUrlToAnalyze) {
        console.error('No image URL provided');
        return;
      }
        // Fetch the blob data
        const response = await fetch(imageUrlToAnalyze);
        const blobData = await response.blob();

        // Create a FormData object to send the file
        const formData = new FormData();
        formData.append('file', blobData, 'image.jpg');

        // Send the image to your backend API
        const uploadResponse = await axios.post('http://localhost:5000/api/Image/upload', formData, {
          headers: {
            'Content-Type': 'multipart/form-data'
          }
        }
      )   
      }
    }
  }

</script>