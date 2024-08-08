<template>
    <div class="record-results">

      <div class="record-details-flexbox"> 

          <div class="record-icon">
              <img class="record-thumbnail" src="https://th.bing.com/th/id/OIP.m-fEoWyXhoqu8z1MHqIavQHaHa?rs=1&pid=ImgDetMain"> 
          </div>

          <div class="record-information">
              <div class="record-text">Fleetwod Mac</div>
              <div class="record-text">Rumours</div>
          </div>

      </div>

      <div class="price-add">
        
        <div class="price-value"> £23.80</div>
        <div class="add-to-collection">
          <img class="add-record-thumbnail" src="https://cdn-icons-png.flaticon.com/512/4369/4369555.png">
        </div>

      </div>
      
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
    };
  </script>
    
  <style>
    @import '@/assets/css/recordCapture/recordresult.css';
  </style>