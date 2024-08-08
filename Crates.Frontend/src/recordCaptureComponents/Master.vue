
<template>     
  <!-- Header -->
  <Header />
  <div class="master-container">
          



    <!-- Contains everything within the camera output frame. eventually all elements would be placed in here -->
    <div class="media-parent-container">

      <div class="media-child-container"> 

      <!-- The Webcam shows the passenger, giving the choice to take a photo or upload one -->
      <div class="camera-container" v-if="showCamera && !showUploadScreen">

        <!-- Frame to guide passenger to correctly align face before taking photo -->
        <div class="camera-face-overlay">
          <div class="top-left-corner"></div>
          <div class="top-right-corner"></div>
          <div class="bottom-left-corner"></div>
          <div class="bottom-right-corner"></div>
        </div>       
        
        <!-- Camera component -->
        <Camera ref="cameraComponent" @photo-taken="photoTaken"/>

        <!-- Take photo / Upload photo buttons -->
        <div>
          <button v-if="showCamera && !showUploadScreen" class="action-button take-photo" @click="capturePhoto">
            <i class="bi bi-camera bootstrap-camera-icon-style"></i>
          </button>
        </div>
      </div>

      
      <div v-if="showImageSelector" class="image-container"> 

          <!-- Image selector component replaces the uploaded image view -->
          <ImageSelector 
            v-if="showImageSelector"
            ref="ImageSelector"
            :imageUrl="capturedImageUrl"
            @selection-processed="processSelectedImage"
            @selection-cancelled="cancelUpload"
          />

                <!-- Image Analysis-->
            <ImageAnalysis 
                ref="imageAnalysisComponent"
                :imageUrl="processedImageUrl || imageUrl"
                :triggerAnalysis="triggerImageAnalysis"
            />    
              
          <div v-if="showImageSelector" class="preview-button-container">
            <div class="shape-choice-flex">
              <button 
                class="rectangle-select-button" 
                :class="{ 'selected': selectedTool === 'rectangle' }" 
                @click="setTool('rectangle')">
                <i class="bi bi-square"></i>
              </button>
              <button 
                class="freeform-select-button" 
                :class="{ 'selected': selectedTool === 'freeform' }" 
                @click="setTool('freeform')">
                <i class="bi bi-paint-bucket"></i>
              </button>
              <button class="clear-selection-button" @click="clearSelection">
                <i class="bi bi-x-circle"></i>
              </button>
            </div>
          <div id="discover-toggles">
            <button type="button" @click="cancelSelection">Retake</button>
            <button type="button" @click="processSelection">Process</button> 
          </div>
          </div>
        </div>
    </div>
  </div>

  <!-- Footer which contains upload image button. Removed for demo -->
  <!-- <Footer @file-selected="selectImage" />

  <!-- Logstream -->
  <!-- <LogStream :logMessage="currentlogMessage" /> -->
  <RecordResult />

  </div>
</template>

<script lang="ts">
import { defineComponent } from "vue";
import Camera from "@/recordCaptureComponents/Camera.vue";
import Header from "@/recordCaptureComponents/Header.vue";
import ImageAnalysis from "@/recordCaptureComponents/ImageAnalysis.vue";
import ImageSelector from '@/recordCaptureComponents/ImageSelector.vue';
import LogStream from '@/recordCaptureComponents/LogStream.vue';
import RecordResult from '@/recordCaptureComponents/RecordResult.vue';


export default defineComponent({
  components: {
    Camera,
    ImageAnalysis,
    ImageSelector,
    Header,
    LogStream,
    RecordResult,
  },

  data() {
    return {
      // Stores the URL of the uploaded image
      imageUrl: "",

      // Flag to indicate whether to show the camera or not
      showCamera: true,

      // Flag to indicate whether to show the upload screen or not
      showUploadScreen: false,

      // Used by the LogStream component to display the most recent log message
      currentlogMessage: null,

      // A list of the captured images
      capturedImage: null,

      // A list of the image blob urls
      imageDataUrl: null,
      
      // Add new data properties for the window width and height
      windowWidth: window.innerWidth,
      windowHeight: window.innerHeight,

      capturedImageUrl: '',
      processedImageUrl: '',
      showImageSelector: false,
      showImageAnalysis: false,

      // Selected tool state
      selectedTool: 'rectangle'
    };
  },
  methods: {
    processSelection(){
      this.$refs.ImageSelector.processSelection();
    },
    cancelSelection(){
      this.$refs.ImageSelector.cancelSelection();
    },
    clearSelection(){
      this.$refs.ImageSelector.clearSelection();
    },
    setTool(tool: string){
      this.selectedTool = tool;
      this.$refs.ImageSelector.setTool(tool);
    },
    // Allows the take photo button to emit an event to the camera component
    capturePhoto() {
      this.$refs.cameraComponent.capturePhoto();
    },
    triggerImageAnalysis() {
      // This method will be called when the "Analyze Image" button is clicked
      this.$refs.imageAnalysisComponent.analyzeImage();
    },
    // Takes the emitted event from the camera module to change the screen to the preview
    photoTaken({ data }) {
      const img = new Image();
      img.onload = () => {
        const canvas = document.createElement('canvas');
        canvas.width = img.width;
        canvas.height = img.height;
        const ctx = canvas.getContext('2d');
        ctx.drawImage(img, 0, 0);
        canvas.toBlob((blob) => {
          this.imageDataUrl = URL.createObjectURL(blob);
          this.imageUrl = this.imageDataUrl;

          this.capturedImageUrl = data;
          this.showCamera = false;
          this.showImageSelector = true;
          this.changeLogItem("Snapshot taken from camera");
        }, 'image/jpeg', 0.95); // Convert to JPEG with 95% quality
      };
      img.src = data;
    },
    processSelectedImage(selectedImageUrl) {
      fetch(selectedImageUrl)
      .then(res => res.blob())
      .then(blob => {
        this.processedImageBlob = blob;
        this.processedImageUrl = URL.createObjectURL(blob);

        // Ensure the analyzeImage method is called with the correct image URL
        const imageUrlToAnalyze = this.processedImageUrl || this.imageUrl;
        this.$refs.imageAnalysisComponent.analyzeImage(imageUrlToAnalyze);

        this.showUploadScreen = false;
        this.showCamera = true;
        this.showImageSelector = false;

      })
    },
    delay(ms: number) {
      return new Promise(resolve => setTimeout(resolve, ms));
    },
    // Handles the change event which displays the uploaded photo on preview screen
    selectImage(event: Event) {
      const canvasWidth = 300;
      const canvasHeight = 550
      const inputElement = event.target as HTMLInputElement;
      const file = inputElement.files?.[0];
      if (file) {
        const reader = new FileReader();
        reader.onload = (e) => {
          const img = new Image();
          img.onload = () => {
            const canvas = document.createElement('canvas');
            canvas.width = canvasWidth;
            canvas.height = canvasHeight;
            const ctx = canvas.getContext('2d');
            
            // Draw the image directly onto the canvas, cropping if necessary
            ctx.drawImage(img, 0, 0, canvasWidth, canvasHeight);
            
            this.imageDataUrl = canvas.toDataURL('image/jpeg');
            this.imageUrl = this.imageDataUrl;
            this.showUploadScreen = true;
            this.showCamera = false;
            this.selectedImage = file;
            this.changeLogItem(`Uploaded image: ${file.name}`);
          };
          img.src = e.target.result as string;
        };
        reader.readAsDataURL(file);
      }
    },
    // Handles the cancel event
    cancelFileSelection() {
      this.changeLogItem("Upload cancelled, returning to camera.");
    },
    // Cancels the preview to allow the user to take/upload another photo
    cancelUpload() {
      this.showUploadScreen = false;
      this.showCamera = true;
      this.showImageSelector = false;
      this.capturedImageUrl = '';
      this.processedImageUrl = '';
      this.changeLogItem("Cancelled, going back to camera");
    },
    // Pushes the event to to the logstream
    changeLogItem(message: string) {
      this.currentlogMessage = message;
    },
  }
});
</script>


<style scoped>
  @import '@/assets/css/recordCapture/master.css';
</style>
