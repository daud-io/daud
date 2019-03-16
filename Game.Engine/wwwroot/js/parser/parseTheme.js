const antlr4=require("antlr4");
var ScssLexer= require("./ScssLexer").ScssLexer;
var ScssParser= require("./ScssParser").ScssParser;

var input = '.test{background:"green"}';
var chars = new antlr4.InputStream(input);
var lexer = new ScssLexer(chars);
var tokens  = new antlr4.CommonTokenStream(lexer);
var parser = new ScssParser(tokens);
var tree = parser.stylesheet();
console.log(tree);

//i am gonna push what we have