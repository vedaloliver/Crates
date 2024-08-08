module.exports = {
    devServer: {
      proxy: {
        '/api': {
          target: 'https://null.azurewebsites.net',
          changeOrigin: true,
          pathRewrite: {
            '^/api': ''
          }
        }
      }
    }
  };