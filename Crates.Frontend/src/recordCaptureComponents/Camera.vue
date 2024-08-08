<template>
    <div>
      <div class="camera">
        <video v-if="!photoData" ref="video" class="camera-feed" autoplay playsInline muted>Video Stream.</video>
      </div>
    </div>
  </template>
  
  <script>
  export default {
    data() {
      return {
        photoData: null,
        video: null,
        definedHeight: 0,
        definedWidth: 720,
      };
    },
    mounted() {
      this.startup();
    },
    methods: {

      // Sets up the camera feed
      startup() {
        let height = 0;
        let streaming = false;
        this.video = this.$refs.video;
        this.definedHeight = (this.video.videoHeight / this.video.videoWidth) * this.definedWidth;
        this.definedHeight = this.definedHeight + 200
  
        navigator.mediaDevices
          .getUserMedia({ video: true, audio: false })
          .then((stream) => {
            this.video.srcObject = stream;
            this.video.play();
          })
          .catch((err) => {
            console.error(`An error occurred: ${err}`);
          });
  
        this.video.addEventListener(
          'canplay',
          (ev) => {
            if (!streaming) {
              this.definedHeight = (this.video.videoHeight / this.video.videoWidth) * this.definedWidth;
              height = this.definedHeight;
  
              this.video.setAttribute('width', this.definedWidth);
              this.video.setAttribute('height', this.definedHeight);
              streaming = true;
            }
          },
          false
        );
      },

      // Captures the photo
      capturePhoto() {
      const canvas = document.createElement('canvas');
      const context = canvas.getContext('2d');
      canvas.width = this.definedWidth;
      canvas.height = this.definedHeight;

      context.drawImage(this.video, 0, 0, this.definedWidth, this.definedHeight);
      
      // Convert canvas to blob
      canvas.toBlob((blob) => {
        // Create a URL for the blob
        const url = URL.createObjectURL(blob);
        this.$emit('photo-taken', { data: url, blob: blob });
      }, 'image/png');
  }
}
  }
  </script>

<style scoped>
.camera-feed {
  border-radius: 35%; 
  overflow: hidden;
}
</style>
  