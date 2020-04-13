import Plotly = require("plotly.js/lib/core");
import barpolar = require("plotly.js/lib/barpolar");
// Load in the trace types for barpolar
Plotly.register([barpolar]);

export default Plotly;
