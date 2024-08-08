<template>
    <div class="image-selector">
      <div class="image-container" ref="imageContainer">
        <img :src="imageUrl" alt="Captured image" @load="onImageLoad">
        <canvas ref="overlayCanvas" 
                @mousedown="startSelection" 
                @mousemove="updateSelection" 
                @mouseup="endSelection"
                @mouseout="endSelection"></canvas>

      <!-- Image Analysis-->
      <ImageAnalysis 
          ref="imageAnalysisComponent"
          :imageUrl="processedImageUrl || imageUrl"
          :triggerAnalysis="triggerImageAnalysis"
      />     
      </div>
    </div>
  </template>
  
  <script>
  import ImageAnalysis from "@/recordCaptureComponents/ImageAnalysis.vue";

  export default {
    name: 'ImageSelector',
    props: {
      imageUrl: {
        type: String,
        required: true
      }
    },
    components: {
      ImageAnalysis
    },
    data() {
      return {
        ctx: null,
        isSelecting: false,
        startX: 0,
        startY: 0,
        currentTool: 'rectangle',
        selections: [],
        freeformPath: []
      }
    },
    methods: {
      onImageLoad() {
        this.initCanvas();
      },
      initCanvas() {
        const canvas = this.$refs.overlayCanvas;
        const img = this.$refs.imageContainer.querySelector('img');
        canvas.width = img.width;
        canvas.height = img.height;
        this.ctx = canvas.getContext('2d');
        this.ctx.fillStyle = 'rgba(0, 0, 0, 0.3)';
        this.ctx.fillRect(0, 0, canvas.width, canvas.height);
      },
      setTool(tool) {
        this.currentTool = tool;
      },
      startSelection(e) {
        this.isSelecting = true;
        const rect = e.target.getBoundingClientRect();
        this.startX = e.clientX - rect.left;
        this.startY = e.clientY - rect.top;
        if (this.currentTool === 'freeform') {
          this.freeformPath = [{x: this.startX, y: this.startY}];
        }
      },
      updateSelection(e) {
        if (!this.isSelecting) return;
        
        const rect = e.target.getBoundingClientRect();
        const endX = e.clientX - rect.left;
        const endY = e.clientY - rect.top;
  
        this.ctx.clearRect(0, 0, this.ctx.canvas.width, this.ctx.canvas.height);
        this.ctx.fillRect(0, 0, this.ctx.canvas.width, this.ctx.canvas.height);
  
        // Draw existing selections
        this.drawSelections();
  
        if (this.currentTool === 'rectangle') {
          this.ctx.clearRect(this.startX, this.startY, endX - this.startX, endY - this.startY);
        } else if (this.currentTool === 'freeform') {
          this.freeformPath.push({x: endX, y: endY});
          this.drawFreeformPath();
        }
      },
      endSelection(e) {
        if (!this.isSelecting) return;
        this.isSelecting = false;
        const rect = e.target.getBoundingClientRect();
        const endX = e.clientX - rect.left;
        const endY = e.clientY - rect.top;
  
        if (this.currentTool === 'rectangle') {
          this.selections.push({
            type: 'rectangle',
            startX: this.startX,
            startY: this.startY,
            endX: endX,
            endY: endY
          });
        } else if (this.currentTool === 'freeform') {
          this.selections.push({
            type: 'freeform',
            path: [...this.freeformPath]
          });
          this.freeformPath = [];
        }
      },
      drawSelections() {
        this.selections.forEach(selection => {
          if (selection.type === 'rectangle') {
            this.ctx.clearRect(selection.startX, selection.startY, 
                               selection.endX - selection.startX, 
                               selection.endY - selection.startY);
          } else if (selection.type === 'freeform') {
            this.drawFreeformPath(selection.path);
          }
        });
      },
      drawFreeformPath(path = this.freeformPath) {
        if (path.length < 2) return;
        
        this.ctx.save();
        this.ctx.beginPath();
        this.ctx.moveTo(path[0].x, path[0].y);
        for (let i = 1; i < path.length; i++) {
          this.ctx.lineTo(path[i].x, path[i].y);
        }
        this.ctx.lineWidth = 60; // 30px on each side
        this.ctx.lineCap = 'round';
        this.ctx.lineJoin = 'round';
        this.ctx.globalCompositeOperation = 'destination-out';
        this.ctx.stroke();
        this.ctx.restore();
      },
      clearSelection() {
        this.selections = [];
        this.initCanvas();
      },
      cancelSelection() {
        this.$emit('selection-cancelled');
      },
      async processSelection() {
        const canvas = document.createElement('canvas');
        const img = this.$refs.imageContainer.querySelector('img');
        canvas.width = img.width;
        canvas.height = img.height;
        const ctx = canvas.getContext('2d');
        ctx.drawImage(img, 0, 0);
  
        // Create a new canvas with only the selected areas
        const selectedCanvas = document.createElement('canvas');
        selectedCanvas.width = img.width;
        selectedCanvas.height = img.height;
        const selectedCtx = selectedCanvas.getContext('2d');
  
        this.selections.forEach(selection => {
          if (selection.type === 'rectangle') {
            const imageData = ctx.getImageData(selection.startX, selection.startY, 
                                               selection.endX - selection.startX, 
                                               selection.endY - selection.startY);
            selectedCtx.putImageData(imageData, selection.startX, selection.startY);
          } else if (selection.type === 'freeform') {
            selectedCtx.save();
            selectedCtx.beginPath();
            selectedCtx.moveTo(selection.path[0].x, selection.path[0].y);
            for (let i = 1; i < selection.path.length; i++) {
              selectedCtx.lineTo(selection.path[i].x, selection.path[i].y);
            }
            selectedCtx.lineWidth = 20; // 30px on each side
            selectedCtx.lineCap = 'round';
            selectedCtx.lineJoin = 'round';
            selectedCtx.clip();
            selectedCtx.drawImage(img, 0, 0);
            selectedCtx.restore();
          }
        });
  
        // Convert the canvas to a data URL
        const dataUrl = selectedCanvas.toDataURL('image/png');
  
        // Emit the selected image data URL to the parent component
        this.$emit('selection-processed', dataUrl);
      }
    }
  }
  </script>
  
  <style scoped>
  .image-selector {
    position: relative;
  }
  
  .image-container {
    position: relative;
  }
  
  canvas {
    position: absolute;
    top: 0;
    left: 0;
  }
  
  .controls {
    margin-top: 10px;
  }
  </style>