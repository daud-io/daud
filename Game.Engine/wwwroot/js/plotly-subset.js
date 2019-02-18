var Plotly = require('plotly.js/lib/core');

// Load in the trace types for barpolar
Plotly.register([
    require('plotly.js/lib/barpolar')
]);

module.exports = Plotly;