const antlr4 = require("antlr4");
const CSSselect = require("css-select");
var ScssLexer = require("./ScssLexer").ScssLexer;
var ScssParser = require("./ScssParser").ScssParser;
var input = `#ship {
   textures: 'ship_gray';
 }
 #ship.gray {
   textures: 'ship_gray';
 }
 #ship.bot {
   textures: 'ship0';
 }
 #ship.secret {
   textures: 'ship_secret';
 }
 #ship.zed {
   textures: 'ship_zed';
 }
 #ship.cyan {
   textures: 'ship_cyan';
 }
 #ship.boost {
   textures: inherit, 'thruster_cyan';
 }
 #ship.blue {
   textures: 'ship_blue';
 }
 #ship.blue.boost {
   textures: inherit, 'thruster_blue';
 }
 #ship.green {
   textures: 'ship_green';
 }
 #ship.green.boost {
   textures: inherit, 'thruster_green';
 }
 #ship.orange {
   textures: 'ship_orange';
 }
 #ship.orange.boost {
   textures: inherit, 'thruster_orange';
 }
 #ship.pink {
   textures: 'ship_pink';
 }
 #ship.pink.boost {
   textures: inherit, 'thruster_pink';
 }
 #ship.red {
   textures: 'ship_red';
 }
 #ship.red.boost {
   textures: inherit, 'thruster_red';
 }
 #ship.yellow {
   textures: 'ship_yellow';
 }
 #ship.yellow.boost {
   textures: inherit, 'thruster_yellow';
 }
 #ship.offenseupgrade {
   textures: inherit, 'offenseupgrade';
 }
 #ship.defenseupgrade {
   textures: inherit, 'defenseupgrade';
 }
 #ship.invulnerable {
   textures: inherit, 'invulnerable';
 }
 #ship.shield {
   textures: inherit, 'shield';
 }
 #bullet {
   textures: 'bullet';
 }
 #bullet.cyan {
   textures: 'bullet_cyan';
 }
 #bullet.blue {
   textures: 'bullet_blue';
 }
 #bullet.green {
   textures: 'bullet_green';
 }
 #bullet.orange {
   textures: 'bullet_orange';
 }
 #bullet.pink {
   textures: 'bullet_pink';
 }
 #bullet.red {
   textures: 'bullet_red';
 }
 #bullet.yellow {
   textures: 'bullet_yellow';
 }
 #obstacle {
   textures: 'obstacle';
 }
 #wormhole {
   textures: 'wormhole';
 }
 #seeker {
   textures: 'seeker';
 }
 #pickup.seeker {
   textures: 'seeker_pickup';
 }
 #pickup.shield {
   textures: 'shield_pickup';
 }
 #ctf_base {
   textures: 'ctf_base';
 }
 #ctf_flag.blue {
   textures: 'ctf_flag_blue';
 }
 #ctf_flag.red {
   textures: 'ctf_flag_red';
 }
 #map {
   textures: 'map';
 }
  `;

function parseCssIntoRules(css) {
   var chars = new antlr4.InputStream(css);
   var lexer = new ScssLexer(chars);
   var tokens = new antlr4.CommonTokenStream(lexer);
   var parser = new ScssParser(tokens);
   var tree = parser.stylesheet();

   var ruleList = [];
   function addRulesFromStatement(statement, rules) {
      var selectors = statement.ruleset().selectors();
      var block = statement.ruleset().block();

      var blockProps = block.property().map((x, i) => [block.property(i).identifier().getText(), block.property(i).values().children.map(x => x.children ? x.children.map(y => y.getText()).join(" ") : x.getText()).join(" ")]);//this
      var blockOBJ = {};

      for (var i = 0; i < blockProps.length; i++) {
         blockOBJ[blockProps[i][0]] = blockProps[i][1];
      }
      for (var i = 0; i < selectors.children.length; i++) {
         rules.push({ selector: selectors.selector(i).getText(), obj: blockOBJ });
      }
   }

   //nested select is still broken but should be an easy fix ( or could just compile to css)
   for (var i = 0; i < tree.children.length; i++) {
      addRulesFromStatement(tree.children[i], ruleList);
   }
   return ruleList;
}
var ruleList = parseCssIntoRules(input);

function selectorMatches(selector, selectProps) {
   var thing = {
      type: 'tag',
      name: selectProps.element,
      attribs: {
         id: selectProps.id,
         class: selectProps.class
      }
   };
   return CSSselect.is(thing, selector);

}


//var queryProperties({id: "ship", class: ".cyan", modifiers: [ "[boost=1]" ] });
function queryProperties(element) {
   var res = {};
   for (var i = 0; i < ruleList.length; i++) {
      if (selectorMatches(ruleList[i].selector, element)) {
         //console.log("MATCH",ruleList[i].selector,element);
         for (var p in ruleList[i].obj) {

            if (res[p] == undefined) {
               res[p] = "''";
            }
            res[p] = ruleList[i].obj[p].replace("inherit", res[p]);
         }
      }
   }
   return res;
}

function getShipProperties(ship, more) {
   return queryProperties({ id: "ship", class: ship + " " + more.join(" "), })
}
console.log(getShipProperties("cyan", ["boost", "defenseupgrade"]))
