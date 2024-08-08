<template>
  <div ref="logText" class="log-text rubik-font">{{ logMessage }}
    <div>Result</div>
    <div>image</div>
    <div>image</div>
    <div>image</div>  
  </div>
</template>

<script>
  export default {
    props: {
      logMessage: {
        type: String,
        required: true,
      },
    },
    updated() {
      this.updateLogText();
    },
    methods: {
      updateLogText() {
        const logText = this.$refs.logText;
        logText.innerText = this.logMessage;
      },
    },
  };
</script>
  
<style>
  @import '@/assets/css/recordCapture/logstream.css';
</style>