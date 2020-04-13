// Generated from src/parser/ScssParser.g4 by ANTLR 4.7.3-SNAPSHOT


import { ATN } from "antlr4ts/atn/ATN";
import { ATNDeserializer } from "antlr4ts/atn/ATNDeserializer";
import { FailedPredicateException } from "antlr4ts/FailedPredicateException";
import { NotNull } from "antlr4ts/Decorators";
import { NoViableAltException } from "antlr4ts/NoViableAltException";
import { Override } from "antlr4ts/Decorators";
import { Parser } from "antlr4ts/Parser";
import { ParserRuleContext } from "antlr4ts/ParserRuleContext";
import { ParserATNSimulator } from "antlr4ts/atn/ParserATNSimulator";
import { ParseTreeListener } from "antlr4ts/tree/ParseTreeListener";
import { ParseTreeVisitor } from "antlr4ts/tree/ParseTreeVisitor";
import { RecognitionException } from "antlr4ts/RecognitionException";
import { RuleContext } from "antlr4ts/RuleContext";
//import { RuleVersion } from "antlr4ts/RuleVersion";
import { TerminalNode } from "antlr4ts/tree/TerminalNode";
import { Token } from "antlr4ts/Token";
import { TokenStream } from "antlr4ts/TokenStream";
import { Vocabulary } from "antlr4ts/Vocabulary";
import { VocabularyImpl } from "antlr4ts/VocabularyImpl";

import * as Utils from "antlr4ts/misc/Utils";

import { ScssParserListener } from "./ScssParserListener";
import { ScssParserVisitor } from "./ScssParserVisitor";


export class ScssParser extends Parser {
	public static readonly NULL = 1;
	public static readonly IN = 2;
	public static readonly Unit = 3;
	public static readonly COMBINE_COMPARE = 4;
	public static readonly Ellipsis = 5;
	public static readonly InterpolationStart = 6;
	public static readonly LPAREN = 7;
	public static readonly RPAREN = 8;
	public static readonly BlockStart = 9;
	public static readonly BlockEnd = 10;
	public static readonly LBRACK = 11;
	public static readonly RBRACK = 12;
	public static readonly GT = 13;
	public static readonly TIL = 14;
	public static readonly LT = 15;
	public static readonly COLON = 16;
	public static readonly SEMI = 17;
	public static readonly COMMA = 18;
	public static readonly DOT = 19;
	public static readonly DOLLAR = 20;
	public static readonly AT = 21;
	public static readonly AND = 22;
	public static readonly HASH = 23;
	public static readonly COLONCOLON = 24;
	public static readonly PLUS = 25;
	public static readonly TIMES = 26;
	public static readonly DIV = 27;
	public static readonly MINUS = 28;
	public static readonly PERC = 29;
	public static readonly UrlStart = 30;
	public static readonly EQEQ = 31;
	public static readonly NOTEQ = 32;
	public static readonly EQ = 33;
	public static readonly PIPE_EQ = 34;
	public static readonly TILD_EQ = 35;
	public static readonly MIXIN = 36;
	public static readonly FUNCTION = 37;
	public static readonly AT_ELSE = 38;
	public static readonly IF = 39;
	public static readonly AT_IF = 40;
	public static readonly AT_FOR = 41;
	public static readonly AT_WHILE = 42;
	public static readonly AT_EACH = 43;
	public static readonly INCLUDE = 44;
	public static readonly IMPORT = 45;
	public static readonly RETURN = 46;
	public static readonly FROM = 47;
	public static readonly THROUGH = 48;
	public static readonly POUND_DEFAULT = 49;
	public static readonly Identifier = 50;
	public static readonly StringLiteral = 51;
	public static readonly Number = 52;
	public static readonly Color = 53;
	public static readonly WS = 54;
	public static readonly SL_COMMENT = 55;
	public static readonly COMMENT = 56;
	public static readonly UrlEnd = 57;
	public static readonly Url = 58;
	public static readonly SPACE = 59;
	public static readonly InterpolationStartAfter = 60;
	public static readonly IdentifierAfter = 61;
	public static readonly RULE_stylesheet = 0;
	public static readonly RULE_statement = 1;
	public static readonly RULE_params = 2;
	public static readonly RULE_param = 3;
	public static readonly RULE_variableName = 4;
	public static readonly RULE_paramOptionalValue = 5;
	public static readonly RULE_mixinDeclaration = 6;
	public static readonly RULE_includeDeclaration = 7;
	public static readonly RULE_functionDeclaration = 8;
	public static readonly RULE_functionBody = 9;
	public static readonly RULE_functionReturn = 10;
	public static readonly RULE_functionStatement = 11;
	public static readonly RULE_commandStatement = 12;
	public static readonly RULE_mathCharacter = 13;
	public static readonly RULE_mathStatement = 14;
	public static readonly RULE_expression = 15;
	public static readonly RULE_ifDeclaration = 16;
	public static readonly RULE_elseIfStatement = 17;
	public static readonly RULE_elseStatement = 18;
	public static readonly RULE_conditions = 19;
	public static readonly RULE_condition = 20;
	public static readonly RULE_variableDeclaration = 21;
	public static readonly RULE_forDeclaration = 22;
	public static readonly RULE_fromNumber = 23;
	public static readonly RULE_throughNumber = 24;
	public static readonly RULE_whileDeclaration = 25;
	public static readonly RULE_eachDeclaration = 26;
	public static readonly RULE_eachValueList = 27;
	public static readonly RULE_identifierListOrMap = 28;
	public static readonly RULE_identifierValue = 29;
	public static readonly RULE_importDeclaration = 30;
	public static readonly RULE_referenceUrl = 31;
	public static readonly RULE_mediaTypes = 32;
	public static readonly RULE_nested = 33;
	public static readonly RULE_nest = 34;
	public static readonly RULE_ruleset = 35;
	public static readonly RULE_block = 36;
	public static readonly RULE_selectors = 37;
	public static readonly RULE_selector = 38;
	public static readonly RULE_selectorPrefix = 39;
	public static readonly RULE_element = 40;
	public static readonly RULE_pseudo = 41;
	public static readonly RULE_attrib = 42;
	public static readonly RULE_attribRelate = 43;
	public static readonly RULE_identifier = 44;
	public static readonly RULE_identifierPart = 45;
	public static readonly RULE_identifierVariableName = 46;
	public static readonly RULE_property = 47;
	public static readonly RULE_values = 48;
	public static readonly RULE_url = 49;
	public static readonly RULE_measurement = 50;
	public static readonly RULE_functionCall = 51;
	// tslint:disable:no-trailing-whitespace
	public static readonly ruleNames: string[] = [
		"stylesheet", "statement", "params", "param", "variableName", "paramOptionalValue", 
		"mixinDeclaration", "includeDeclaration", "functionDeclaration", "functionBody", 
		"functionReturn", "functionStatement", "commandStatement", "mathCharacter", 
		"mathStatement", "expression", "ifDeclaration", "elseIfStatement", "elseStatement", 
		"conditions", "condition", "variableDeclaration", "forDeclaration", "fromNumber", 
		"throughNumber", "whileDeclaration", "eachDeclaration", "eachValueList", 
		"identifierListOrMap", "identifierValue", "importDeclaration", "referenceUrl", 
		"mediaTypes", "nested", "nest", "ruleset", "block", "selectors", "selector", 
		"selectorPrefix", "element", "pseudo", "attrib", "attribRelate", "identifier", 
		"identifierPart", "identifierVariableName", "property", "values", "url", 
		"measurement", "functionCall",
	];

	private static readonly _LITERAL_NAMES: Array<string | undefined> = [
		undefined, "'null'", "'in'", undefined, undefined, "'...'", undefined, 
		"'('", "')'", "'{'", "'}'", "'['", "']'", "'>'", "'~'", "'<'", "':'", 
		"';'", "','", "'.'", "'$'", "'@'", "'&'", "'#'", "'::'", "'+'", "'*'", 
		"'/'", "'-'", "'%'", undefined, "'=='", "'!='", "'='", "'|='", "'~='", 
		"'@mixin'", "'@function'", "'@else'", "'if'", "'@if'", "'@for'", "'@while'", 
		"'@each'", "'@include'", "'@import'", "'@return'", "'from'", "'through'", 
		"'!default'",
	];
	private static readonly _SYMBOLIC_NAMES: Array<string | undefined> = [
		undefined, "NULL", "IN", "Unit", "COMBINE_COMPARE", "Ellipsis", "InterpolationStart", 
		"LPAREN", "RPAREN", "BlockStart", "BlockEnd", "LBRACK", "RBRACK", "GT", 
		"TIL", "LT", "COLON", "SEMI", "COMMA", "DOT", "DOLLAR", "AT", "AND", "HASH", 
		"COLONCOLON", "PLUS", "TIMES", "DIV", "MINUS", "PERC", "UrlStart", "EQEQ", 
		"NOTEQ", "EQ", "PIPE_EQ", "TILD_EQ", "MIXIN", "FUNCTION", "AT_ELSE", "IF", 
		"AT_IF", "AT_FOR", "AT_WHILE", "AT_EACH", "INCLUDE", "IMPORT", "RETURN", 
		"FROM", "THROUGH", "POUND_DEFAULT", "Identifier", "StringLiteral", "Number", 
		"Color", "WS", "SL_COMMENT", "COMMENT", "UrlEnd", "Url", "SPACE", "InterpolationStartAfter", 
		"IdentifierAfter",
	];
	public static readonly VOCABULARY: Vocabulary = new VocabularyImpl(ScssParser._LITERAL_NAMES, ScssParser._SYMBOLIC_NAMES, []);

	// @Override
	// @NotNull
	public get vocabulary(): Vocabulary {
		return ScssParser.VOCABULARY;
	}
	// tslint:enable:no-trailing-whitespace

	// @Override
	public get grammarFileName(): string { return "ScssParser.g4"; }

	// @Override
	public get ruleNames(): string[] { return ScssParser.ruleNames; }

	// @Override
	public get serializedATN(): string { return ScssParser._serializedATN; }

	constructor(input: TokenStream) {
		super(input);
		this._interp = new ParserATNSimulator(ScssParser._ATN, this);
	}
	// @RuleVersion(0)
	public stylesheet(): StylesheetContext {
		let _localctx: StylesheetContext = new StylesheetContext(this._ctx, this.state);
		this.enterRule(_localctx, 0, ScssParser.RULE_stylesheet);
		let _la: number;
		try {
			this.enterOuterAlt(_localctx, 1);
			{
			this.state = 107;
			this._errHandler.sync(this);
			_la = this._input.LA(1);
			while ((((_la) & ~0x1F) === 0 && ((1 << _la) & ((1 << ScssParser.InterpolationStart) | (1 << ScssParser.DOT) | (1 << ScssParser.DOLLAR) | (1 << ScssParser.AT) | (1 << ScssParser.AND) | (1 << ScssParser.HASH) | (1 << ScssParser.TIMES))) !== 0) || ((((_la - 36)) & ~0x1F) === 0 && ((1 << (_la - 36)) & ((1 << (ScssParser.MIXIN - 36)) | (1 << (ScssParser.FUNCTION - 36)) | (1 << (ScssParser.AT_IF - 36)) | (1 << (ScssParser.AT_FOR - 36)) | (1 << (ScssParser.AT_WHILE - 36)) | (1 << (ScssParser.AT_EACH - 36)) | (1 << (ScssParser.INCLUDE - 36)) | (1 << (ScssParser.IMPORT - 36)) | (1 << (ScssParser.Identifier - 36)))) !== 0)) {
				{
				{
				this.state = 104;
				this.statement();
				}
				}
				this.state = 109;
				this._errHandler.sync(this);
				_la = this._input.LA(1);
			}
			}
		}
		catch (re) {
			if (re instanceof RecognitionException) {
				_localctx.exception = re;
				this._errHandler.reportError(this, re);
				this._errHandler.recover(this, re);
			} else {
				throw re;
			}
		}
		finally {
			this.exitRule();
		}
		return _localctx;
	}
	// @RuleVersion(0)
	public statement(): StatementContext {
		let _localctx: StatementContext = new StatementContext(this._ctx, this.state);
		this.enterRule(_localctx, 2, ScssParser.RULE_statement);
		try {
			this.state = 121;
			this._errHandler.sync(this);
			switch (this._input.LA(1)) {
			case ScssParser.IMPORT:
				this.enterOuterAlt(_localctx, 1);
				{
				this.state = 110;
				this.importDeclaration();
				}
				break;
			case ScssParser.AT:
				this.enterOuterAlt(_localctx, 2);
				{
				this.state = 111;
				this.nested();
				}
				break;
			case ScssParser.InterpolationStart:
			case ScssParser.DOT:
			case ScssParser.AND:
			case ScssParser.HASH:
			case ScssParser.TIMES:
			case ScssParser.Identifier:
				this.enterOuterAlt(_localctx, 3);
				{
				this.state = 112;
				this.ruleset();
				}
				break;
			case ScssParser.MIXIN:
				this.enterOuterAlt(_localctx, 4);
				{
				this.state = 113;
				this.mixinDeclaration();
				}
				break;
			case ScssParser.FUNCTION:
				this.enterOuterAlt(_localctx, 5);
				{
				this.state = 114;
				this.functionDeclaration();
				}
				break;
			case ScssParser.DOLLAR:
				this.enterOuterAlt(_localctx, 6);
				{
				this.state = 115;
				this.variableDeclaration();
				}
				break;
			case ScssParser.INCLUDE:
				this.enterOuterAlt(_localctx, 7);
				{
				this.state = 116;
				this.includeDeclaration();
				}
				break;
			case ScssParser.AT_IF:
				this.enterOuterAlt(_localctx, 8);
				{
				this.state = 117;
				this.ifDeclaration();
				}
				break;
			case ScssParser.AT_FOR:
				this.enterOuterAlt(_localctx, 9);
				{
				this.state = 118;
				this.forDeclaration();
				}
				break;
			case ScssParser.AT_WHILE:
				this.enterOuterAlt(_localctx, 10);
				{
				this.state = 119;
				this.whileDeclaration();
				}
				break;
			case ScssParser.AT_EACH:
				this.enterOuterAlt(_localctx, 11);
				{
				this.state = 120;
				this.eachDeclaration();
				}
				break;
			default:
				throw new NoViableAltException(this);
			}
		}
		catch (re) {
			if (re instanceof RecognitionException) {
				_localctx.exception = re;
				this._errHandler.reportError(this, re);
				this._errHandler.recover(this, re);
			} else {
				throw re;
			}
		}
		finally {
			this.exitRule();
		}
		return _localctx;
	}
	// @RuleVersion(0)
	public params(): ParamsContext {
		let _localctx: ParamsContext = new ParamsContext(this._ctx, this.state);
		this.enterRule(_localctx, 4, ScssParser.RULE_params);
		let _la: number;
		try {
			this.enterOuterAlt(_localctx, 1);
			{
			this.state = 123;
			this.param();
			this.state = 128;
			this._errHandler.sync(this);
			_la = this._input.LA(1);
			while (_la === ScssParser.COMMA) {
				{
				{
				this.state = 124;
				this.match(ScssParser.COMMA);
				this.state = 125;
				this.param();
				}
				}
				this.state = 130;
				this._errHandler.sync(this);
				_la = this._input.LA(1);
			}
			this.state = 132;
			this._errHandler.sync(this);
			_la = this._input.LA(1);
			if (_la === ScssParser.Ellipsis) {
				{
				this.state = 131;
				this.match(ScssParser.Ellipsis);
				}
			}

			}
		}
		catch (re) {
			if (re instanceof RecognitionException) {
				_localctx.exception = re;
				this._errHandler.reportError(this, re);
				this._errHandler.recover(this, re);
			} else {
				throw re;
			}
		}
		finally {
			this.exitRule();
		}
		return _localctx;
	}
	// @RuleVersion(0)
	public param(): ParamContext {
		let _localctx: ParamContext = new ParamContext(this._ctx, this.state);
		this.enterRule(_localctx, 6, ScssParser.RULE_param);
		let _la: number;
		try {
			this.enterOuterAlt(_localctx, 1);
			{
			this.state = 134;
			this.variableName();
			this.state = 136;
			this._errHandler.sync(this);
			_la = this._input.LA(1);
			if (_la === ScssParser.COLON) {
				{
				this.state = 135;
				this.paramOptionalValue();
				}
			}

			}
		}
		catch (re) {
			if (re instanceof RecognitionException) {
				_localctx.exception = re;
				this._errHandler.reportError(this, re);
				this._errHandler.recover(this, re);
			} else {
				throw re;
			}
		}
		finally {
			this.exitRule();
		}
		return _localctx;
	}
	// @RuleVersion(0)
	public variableName(): VariableNameContext {
		let _localctx: VariableNameContext = new VariableNameContext(this._ctx, this.state);
		this.enterRule(_localctx, 8, ScssParser.RULE_variableName);
		try {
			this.enterOuterAlt(_localctx, 1);
			{
			this.state = 138;
			this.match(ScssParser.DOLLAR);
			this.state = 139;
			this.match(ScssParser.Identifier);
			}
		}
		catch (re) {
			if (re instanceof RecognitionException) {
				_localctx.exception = re;
				this._errHandler.reportError(this, re);
				this._errHandler.recover(this, re);
			} else {
				throw re;
			}
		}
		finally {
			this.exitRule();
		}
		return _localctx;
	}
	// @RuleVersion(0)
	public paramOptionalValue(): ParamOptionalValueContext {
		let _localctx: ParamOptionalValueContext = new ParamOptionalValueContext(this._ctx, this.state);
		this.enterRule(_localctx, 10, ScssParser.RULE_paramOptionalValue);
		let _la: number;
		try {
			this.enterOuterAlt(_localctx, 1);
			{
			this.state = 141;
			this.match(ScssParser.COLON);
			this.state = 143;
			this._errHandler.sync(this);
			_la = this._input.LA(1);
			do {
				{
				{
				this.state = 142;
				this.expression();
				}
				}
				this.state = 145;
				this._errHandler.sync(this);
				_la = this._input.LA(1);
			} while ((((_la) & ~0x1F) === 0 && ((1 << _la) & ((1 << ScssParser.NULL) | (1 << ScssParser.InterpolationStart) | (1 << ScssParser.DOLLAR) | (1 << ScssParser.UrlStart))) !== 0) || ((((_la - 50)) & ~0x1F) === 0 && ((1 << (_la - 50)) & ((1 << (ScssParser.Identifier - 50)) | (1 << (ScssParser.StringLiteral - 50)) | (1 << (ScssParser.Number - 50)) | (1 << (ScssParser.Color - 50)))) !== 0));
			}
		}
		catch (re) {
			if (re instanceof RecognitionException) {
				_localctx.exception = re;
				this._errHandler.reportError(this, re);
				this._errHandler.recover(this, re);
			} else {
				throw re;
			}
		}
		finally {
			this.exitRule();
		}
		return _localctx;
	}
	// @RuleVersion(0)
	public mixinDeclaration(): MixinDeclarationContext {
		let _localctx: MixinDeclarationContext = new MixinDeclarationContext(this._ctx, this.state);
		this.enterRule(_localctx, 12, ScssParser.RULE_mixinDeclaration);
		let _la: number;
		try {
			this.enterOuterAlt(_localctx, 1);
			{
			this.state = 147;
			this.match(ScssParser.MIXIN);
			this.state = 148;
			this.match(ScssParser.Identifier);
			this.state = 154;
			this._errHandler.sync(this);
			_la = this._input.LA(1);
			if (_la === ScssParser.LPAREN) {
				{
				this.state = 149;
				this.match(ScssParser.LPAREN);
				this.state = 151;
				this._errHandler.sync(this);
				_la = this._input.LA(1);
				if (_la === ScssParser.DOLLAR) {
					{
					this.state = 150;
					this.params();
					}
				}

				this.state = 153;
				this.match(ScssParser.RPAREN);
				}
			}

			this.state = 156;
			this.block();
			}
		}
		catch (re) {
			if (re instanceof RecognitionException) {
				_localctx.exception = re;
				this._errHandler.reportError(this, re);
				this._errHandler.recover(this, re);
			} else {
				throw re;
			}
		}
		finally {
			this.exitRule();
		}
		return _localctx;
	}
	// @RuleVersion(0)
	public includeDeclaration(): IncludeDeclarationContext {
		let _localctx: IncludeDeclarationContext = new IncludeDeclarationContext(this._ctx, this.state);
		this.enterRule(_localctx, 14, ScssParser.RULE_includeDeclaration);
		let _la: number;
		try {
			this.enterOuterAlt(_localctx, 1);
			{
			this.state = 158;
			this.match(ScssParser.INCLUDE);
			this.state = 159;
			this.match(ScssParser.Identifier);
			this.state = 174;
			this._errHandler.sync(this);
			switch (this._input.LA(1)) {
			case ScssParser.SEMI:
				{
				this.state = 160;
				this.match(ScssParser.SEMI);
				}
				break;
			case ScssParser.NULL:
			case ScssParser.InterpolationStart:
			case ScssParser.LPAREN:
			case ScssParser.BlockStart:
			case ScssParser.BlockEnd:
			case ScssParser.DOT:
			case ScssParser.DOLLAR:
			case ScssParser.AT:
			case ScssParser.AND:
			case ScssParser.HASH:
			case ScssParser.TIMES:
			case ScssParser.UrlStart:
			case ScssParser.MIXIN:
			case ScssParser.FUNCTION:
			case ScssParser.AT_IF:
			case ScssParser.AT_FOR:
			case ScssParser.AT_WHILE:
			case ScssParser.AT_EACH:
			case ScssParser.INCLUDE:
			case ScssParser.IMPORT:
			case ScssParser.RETURN:
			case ScssParser.Identifier:
			case ScssParser.StringLiteral:
			case ScssParser.Number:
			case ScssParser.Color:
				{
				this.state = 169;
				this._errHandler.sync(this);
				switch ( this.interpreter.adaptivePredict(this._input, 10, this._ctx) ) {
				case 1:
					{
					this.state = 161;
					this.match(ScssParser.LPAREN);
					this.state = 163;
					this._errHandler.sync(this);
					_la = this._input.LA(1);
					if ((((_la) & ~0x1F) === 0 && ((1 << _la) & ((1 << ScssParser.NULL) | (1 << ScssParser.InterpolationStart) | (1 << ScssParser.LPAREN) | (1 << ScssParser.DOLLAR) | (1 << ScssParser.UrlStart))) !== 0) || ((((_la - 50)) & ~0x1F) === 0 && ((1 << (_la - 50)) & ((1 << (ScssParser.Identifier - 50)) | (1 << (ScssParser.StringLiteral - 50)) | (1 << (ScssParser.Number - 50)) | (1 << (ScssParser.Color - 50)))) !== 0)) {
						{
						this.state = 162;
						this.values();
						}
					}

					this.state = 165;
					this.match(ScssParser.RPAREN);
					this.state = 167;
					this._errHandler.sync(this);
					_la = this._input.LA(1);
					if (_la === ScssParser.SEMI) {
						{
						this.state = 166;
						this.match(ScssParser.SEMI);
						}
					}

					}
					break;
				}
				this.state = 172;
				this._errHandler.sync(this);
				_la = this._input.LA(1);
				if (_la === ScssParser.BlockStart) {
					{
					this.state = 171;
					this.block();
					}
				}

				}
				break;
			default:
				throw new NoViableAltException(this);
			}
			}
		}
		catch (re) {
			if (re instanceof RecognitionException) {
				_localctx.exception = re;
				this._errHandler.reportError(this, re);
				this._errHandler.recover(this, re);
			} else {
				throw re;
			}
		}
		finally {
			this.exitRule();
		}
		return _localctx;
	}
	// @RuleVersion(0)
	public functionDeclaration(): FunctionDeclarationContext {
		let _localctx: FunctionDeclarationContext = new FunctionDeclarationContext(this._ctx, this.state);
		this.enterRule(_localctx, 16, ScssParser.RULE_functionDeclaration);
		let _la: number;
		try {
			this.enterOuterAlt(_localctx, 1);
			{
			this.state = 176;
			this.match(ScssParser.FUNCTION);
			this.state = 177;
			this.match(ScssParser.Identifier);
			this.state = 178;
			this.match(ScssParser.LPAREN);
			this.state = 180;
			this._errHandler.sync(this);
			_la = this._input.LA(1);
			if (_la === ScssParser.DOLLAR) {
				{
				this.state = 179;
				this.params();
				}
			}

			this.state = 182;
			this.match(ScssParser.RPAREN);
			this.state = 183;
			this.match(ScssParser.BlockStart);
			this.state = 185;
			this._errHandler.sync(this);
			_la = this._input.LA(1);
			if ((((_la) & ~0x1F) === 0 && ((1 << _la) & ((1 << ScssParser.NULL) | (1 << ScssParser.InterpolationStart) | (1 << ScssParser.LPAREN) | (1 << ScssParser.DOT) | (1 << ScssParser.DOLLAR) | (1 << ScssParser.AT) | (1 << ScssParser.AND) | (1 << ScssParser.HASH) | (1 << ScssParser.TIMES) | (1 << ScssParser.UrlStart))) !== 0) || ((((_la - 36)) & ~0x1F) === 0 && ((1 << (_la - 36)) & ((1 << (ScssParser.MIXIN - 36)) | (1 << (ScssParser.FUNCTION - 36)) | (1 << (ScssParser.AT_IF - 36)) | (1 << (ScssParser.AT_FOR - 36)) | (1 << (ScssParser.AT_WHILE - 36)) | (1 << (ScssParser.AT_EACH - 36)) | (1 << (ScssParser.INCLUDE - 36)) | (1 << (ScssParser.IMPORT - 36)) | (1 << (ScssParser.RETURN - 36)) | (1 << (ScssParser.Identifier - 36)) | (1 << (ScssParser.StringLiteral - 36)) | (1 << (ScssParser.Number - 36)) | (1 << (ScssParser.Color - 36)))) !== 0)) {
				{
				this.state = 184;
				this.functionBody();
				}
			}

			this.state = 187;
			this.match(ScssParser.BlockEnd);
			}
		}
		catch (re) {
			if (re instanceof RecognitionException) {
				_localctx.exception = re;
				this._errHandler.reportError(this, re);
				this._errHandler.recover(this, re);
			} else {
				throw re;
			}
		}
		finally {
			this.exitRule();
		}
		return _localctx;
	}
	// @RuleVersion(0)
	public functionBody(): FunctionBodyContext {
		let _localctx: FunctionBodyContext = new FunctionBodyContext(this._ctx, this.state);
		this.enterRule(_localctx, 18, ScssParser.RULE_functionBody);
		let _la: number;
		try {
			this.enterOuterAlt(_localctx, 1);
			{
			this.state = 192;
			this._errHandler.sync(this);
			_la = this._input.LA(1);
			while ((((_la) & ~0x1F) === 0 && ((1 << _la) & ((1 << ScssParser.NULL) | (1 << ScssParser.InterpolationStart) | (1 << ScssParser.LPAREN) | (1 << ScssParser.DOT) | (1 << ScssParser.DOLLAR) | (1 << ScssParser.AT) | (1 << ScssParser.AND) | (1 << ScssParser.HASH) | (1 << ScssParser.TIMES) | (1 << ScssParser.UrlStart))) !== 0) || ((((_la - 36)) & ~0x1F) === 0 && ((1 << (_la - 36)) & ((1 << (ScssParser.MIXIN - 36)) | (1 << (ScssParser.FUNCTION - 36)) | (1 << (ScssParser.AT_IF - 36)) | (1 << (ScssParser.AT_FOR - 36)) | (1 << (ScssParser.AT_WHILE - 36)) | (1 << (ScssParser.AT_EACH - 36)) | (1 << (ScssParser.INCLUDE - 36)) | (1 << (ScssParser.IMPORT - 36)) | (1 << (ScssParser.Identifier - 36)) | (1 << (ScssParser.StringLiteral - 36)) | (1 << (ScssParser.Number - 36)) | (1 << (ScssParser.Color - 36)))) !== 0)) {
				{
				{
				this.state = 189;
				this.functionStatement();
				}
				}
				this.state = 194;
				this._errHandler.sync(this);
				_la = this._input.LA(1);
			}
			this.state = 195;
			this.functionReturn();
			}
		}
		catch (re) {
			if (re instanceof RecognitionException) {
				_localctx.exception = re;
				this._errHandler.reportError(this, re);
				this._errHandler.recover(this, re);
			} else {
				throw re;
			}
		}
		finally {
			this.exitRule();
		}
		return _localctx;
	}
	// @RuleVersion(0)
	public functionReturn(): FunctionReturnContext {
		let _localctx: FunctionReturnContext = new FunctionReturnContext(this._ctx, this.state);
		this.enterRule(_localctx, 20, ScssParser.RULE_functionReturn);
		try {
			this.enterOuterAlt(_localctx, 1);
			{
			this.state = 197;
			this.match(ScssParser.RETURN);
			this.state = 198;
			this.commandStatement();
			this.state = 199;
			this.match(ScssParser.SEMI);
			}
		}
		catch (re) {
			if (re instanceof RecognitionException) {
				_localctx.exception = re;
				this._errHandler.reportError(this, re);
				this._errHandler.recover(this, re);
			} else {
				throw re;
			}
		}
		finally {
			this.exitRule();
		}
		return _localctx;
	}
	// @RuleVersion(0)
	public functionStatement(): FunctionStatementContext {
		let _localctx: FunctionStatementContext = new FunctionStatementContext(this._ctx, this.state);
		this.enterRule(_localctx, 22, ScssParser.RULE_functionStatement);
		try {
			this.state = 205;
			this._errHandler.sync(this);
			switch ( this.interpreter.adaptivePredict(this._input, 16, this._ctx) ) {
			case 1:
				this.enterOuterAlt(_localctx, 1);
				{
				this.state = 201;
				this.commandStatement();
				this.state = 202;
				this.match(ScssParser.SEMI);
				}
				break;

			case 2:
				this.enterOuterAlt(_localctx, 2);
				{
				this.state = 204;
				this.statement();
				}
				break;
			}
		}
		catch (re) {
			if (re instanceof RecognitionException) {
				_localctx.exception = re;
				this._errHandler.reportError(this, re);
				this._errHandler.recover(this, re);
			} else {
				throw re;
			}
		}
		finally {
			this.exitRule();
		}
		return _localctx;
	}
	// @RuleVersion(0)
	public commandStatement(): CommandStatementContext {
		let _localctx: CommandStatementContext = new CommandStatementContext(this._ctx, this.state);
		this.enterRule(_localctx, 24, ScssParser.RULE_commandStatement);
		let _la: number;
		try {
			this.enterOuterAlt(_localctx, 1);
			{
			this.state = 216;
			this._errHandler.sync(this);
			switch (this._input.LA(1)) {
			case ScssParser.NULL:
			case ScssParser.InterpolationStart:
			case ScssParser.DOLLAR:
			case ScssParser.UrlStart:
			case ScssParser.Identifier:
			case ScssParser.StringLiteral:
			case ScssParser.Number:
			case ScssParser.Color:
				{
				this.state = 208;
				this._errHandler.sync(this);
				_la = this._input.LA(1);
				do {
					{
					{
					this.state = 207;
					this.expression();
					}
					}
					this.state = 210;
					this._errHandler.sync(this);
					_la = this._input.LA(1);
				} while ((((_la) & ~0x1F) === 0 && ((1 << _la) & ((1 << ScssParser.NULL) | (1 << ScssParser.InterpolationStart) | (1 << ScssParser.DOLLAR) | (1 << ScssParser.UrlStart))) !== 0) || ((((_la - 50)) & ~0x1F) === 0 && ((1 << (_la - 50)) & ((1 << (ScssParser.Identifier - 50)) | (1 << (ScssParser.StringLiteral - 50)) | (1 << (ScssParser.Number - 50)) | (1 << (ScssParser.Color - 50)))) !== 0));
				}
				break;
			case ScssParser.LPAREN:
				{
				this.state = 212;
				this.match(ScssParser.LPAREN);
				this.state = 213;
				this.commandStatement();
				this.state = 214;
				this.match(ScssParser.RPAREN);
				}
				break;
			default:
				throw new NoViableAltException(this);
			}
			this.state = 219;
			this._errHandler.sync(this);
			_la = this._input.LA(1);
			if ((((_la) & ~0x1F) === 0 && ((1 << _la) & ((1 << ScssParser.PLUS) | (1 << ScssParser.TIMES) | (1 << ScssParser.DIV) | (1 << ScssParser.MINUS) | (1 << ScssParser.PERC))) !== 0)) {
				{
				this.state = 218;
				this.mathStatement();
				}
			}

			}
		}
		catch (re) {
			if (re instanceof RecognitionException) {
				_localctx.exception = re;
				this._errHandler.reportError(this, re);
				this._errHandler.recover(this, re);
			} else {
				throw re;
			}
		}
		finally {
			this.exitRule();
		}
		return _localctx;
	}
	// @RuleVersion(0)
	public mathCharacter(): MathCharacterContext {
		let _localctx: MathCharacterContext = new MathCharacterContext(this._ctx, this.state);
		this.enterRule(_localctx, 26, ScssParser.RULE_mathCharacter);
		let _la: number;
		try {
			this.enterOuterAlt(_localctx, 1);
			{
			this.state = 221;
			_la = this._input.LA(1);
			if (!((((_la) & ~0x1F) === 0 && ((1 << _la) & ((1 << ScssParser.PLUS) | (1 << ScssParser.TIMES) | (1 << ScssParser.DIV) | (1 << ScssParser.MINUS) | (1 << ScssParser.PERC))) !== 0))) {
			this._errHandler.recoverInline(this);
			} else {
				if (this._input.LA(1) === Token.EOF) {
					this.matchedEOF = true;
				}

				this._errHandler.reportMatch(this);
				this.consume();
			}
			}
		}
		catch (re) {
			if (re instanceof RecognitionException) {
				_localctx.exception = re;
				this._errHandler.reportError(this, re);
				this._errHandler.recover(this, re);
			} else {
				throw re;
			}
		}
		finally {
			this.exitRule();
		}
		return _localctx;
	}
	// @RuleVersion(0)
	public mathStatement(): MathStatementContext {
		let _localctx: MathStatementContext = new MathStatementContext(this._ctx, this.state);
		this.enterRule(_localctx, 28, ScssParser.RULE_mathStatement);
		try {
			this.enterOuterAlt(_localctx, 1);
			{
			this.state = 223;
			this.mathCharacter();
			this.state = 224;
			this.commandStatement();
			}
		}
		catch (re) {
			if (re instanceof RecognitionException) {
				_localctx.exception = re;
				this._errHandler.reportError(this, re);
				this._errHandler.recover(this, re);
			} else {
				throw re;
			}
		}
		finally {
			this.exitRule();
		}
		return _localctx;
	}
	// @RuleVersion(0)
	public expression(): ExpressionContext {
		let _localctx: ExpressionContext = new ExpressionContext(this._ctx, this.state);
		this.enterRule(_localctx, 30, ScssParser.RULE_expression);
		try {
			this.state = 234;
			this._errHandler.sync(this);
			switch ( this.interpreter.adaptivePredict(this._input, 20, this._ctx) ) {
			case 1:
				this.enterOuterAlt(_localctx, 1);
				{
				this.state = 226;
				this.measurement();
				}
				break;

			case 2:
				this.enterOuterAlt(_localctx, 2);
				{
				this.state = 227;
				this.identifier();
				}
				break;

			case 3:
				this.enterOuterAlt(_localctx, 3);
				{
				this.state = 228;
				this.match(ScssParser.Color);
				}
				break;

			case 4:
				this.enterOuterAlt(_localctx, 4);
				{
				this.state = 229;
				this.match(ScssParser.StringLiteral);
				}
				break;

			case 5:
				this.enterOuterAlt(_localctx, 5);
				{
				this.state = 230;
				this.match(ScssParser.NULL);
				}
				break;

			case 6:
				this.enterOuterAlt(_localctx, 6);
				{
				this.state = 231;
				this.url();
				}
				break;

			case 7:
				this.enterOuterAlt(_localctx, 7);
				{
				this.state = 232;
				this.variableName();
				}
				break;

			case 8:
				this.enterOuterAlt(_localctx, 8);
				{
				this.state = 233;
				this.functionCall();
				}
				break;
			}
		}
		catch (re) {
			if (re instanceof RecognitionException) {
				_localctx.exception = re;
				this._errHandler.reportError(this, re);
				this._errHandler.recover(this, re);
			} else {
				throw re;
			}
		}
		finally {
			this.exitRule();
		}
		return _localctx;
	}
	// @RuleVersion(0)
	public ifDeclaration(): IfDeclarationContext {
		let _localctx: IfDeclarationContext = new IfDeclarationContext(this._ctx, this.state);
		this.enterRule(_localctx, 32, ScssParser.RULE_ifDeclaration);
		let _la: number;
		try {
			let _alt: number;
			this.enterOuterAlt(_localctx, 1);
			{
			this.state = 236;
			this.match(ScssParser.AT_IF);
			this.state = 237;
			this.conditions();
			this.state = 238;
			this.block();
			this.state = 242;
			this._errHandler.sync(this);
			_alt = this.interpreter.adaptivePredict(this._input, 21, this._ctx);
			while (_alt !== 2 && _alt !== ATN.INVALID_ALT_NUMBER) {
				if (_alt === 1) {
					{
					{
					this.state = 239;
					this.elseIfStatement();
					}
					}
				}
				this.state = 244;
				this._errHandler.sync(this);
				_alt = this.interpreter.adaptivePredict(this._input, 21, this._ctx);
			}
			this.state = 246;
			this._errHandler.sync(this);
			_la = this._input.LA(1);
			if (_la === ScssParser.AT_ELSE) {
				{
				this.state = 245;
				this.elseStatement();
				}
			}

			}
		}
		catch (re) {
			if (re instanceof RecognitionException) {
				_localctx.exception = re;
				this._errHandler.reportError(this, re);
				this._errHandler.recover(this, re);
			} else {
				throw re;
			}
		}
		finally {
			this.exitRule();
		}
		return _localctx;
	}
	// @RuleVersion(0)
	public elseIfStatement(): ElseIfStatementContext {
		let _localctx: ElseIfStatementContext = new ElseIfStatementContext(this._ctx, this.state);
		this.enterRule(_localctx, 34, ScssParser.RULE_elseIfStatement);
		try {
			this.enterOuterAlt(_localctx, 1);
			{
			this.state = 248;
			this.match(ScssParser.AT_ELSE);
			this.state = 249;
			this.match(ScssParser.IF);
			this.state = 250;
			this.conditions();
			this.state = 251;
			this.block();
			}
		}
		catch (re) {
			if (re instanceof RecognitionException) {
				_localctx.exception = re;
				this._errHandler.reportError(this, re);
				this._errHandler.recover(this, re);
			} else {
				throw re;
			}
		}
		finally {
			this.exitRule();
		}
		return _localctx;
	}
	// @RuleVersion(0)
	public elseStatement(): ElseStatementContext {
		let _localctx: ElseStatementContext = new ElseStatementContext(this._ctx, this.state);
		this.enterRule(_localctx, 36, ScssParser.RULE_elseStatement);
		try {
			this.enterOuterAlt(_localctx, 1);
			{
			this.state = 253;
			this.match(ScssParser.AT_ELSE);
			this.state = 254;
			this.block();
			}
		}
		catch (re) {
			if (re instanceof RecognitionException) {
				_localctx.exception = re;
				this._errHandler.reportError(this, re);
				this._errHandler.recover(this, re);
			} else {
				throw re;
			}
		}
		finally {
			this.exitRule();
		}
		return _localctx;
	}
	// @RuleVersion(0)
	public conditions(): ConditionsContext {
		let _localctx: ConditionsContext = new ConditionsContext(this._ctx, this.state);
		this.enterRule(_localctx, 38, ScssParser.RULE_conditions);
		try {
			this.state = 262;
			this._errHandler.sync(this);
			switch ( this.interpreter.adaptivePredict(this._input, 24, this._ctx) ) {
			case 1:
				this.enterOuterAlt(_localctx, 1);
				{
				this.state = 256;
				this.condition();
				this.state = 259;
				this._errHandler.sync(this);
				switch ( this.interpreter.adaptivePredict(this._input, 23, this._ctx) ) {
				case 1:
					{
					this.state = 257;
					this.match(ScssParser.COMBINE_COMPARE);
					this.state = 258;
					this.conditions();
					}
					break;
				}
				}
				break;

			case 2:
				this.enterOuterAlt(_localctx, 2);
				{
				this.state = 261;
				this.match(ScssParser.NULL);
				}
				break;
			}
		}
		catch (re) {
			if (re instanceof RecognitionException) {
				_localctx.exception = re;
				this._errHandler.reportError(this, re);
				this._errHandler.recover(this, re);
			} else {
				throw re;
			}
		}
		finally {
			this.exitRule();
		}
		return _localctx;
	}
	// @RuleVersion(0)
	public condition(): ConditionContext {
		let _localctx: ConditionContext = new ConditionContext(this._ctx, this.state);
		this.enterRule(_localctx, 40, ScssParser.RULE_condition);
		let _la: number;
		try {
			this.state = 273;
			this._errHandler.sync(this);
			switch ( this.interpreter.adaptivePredict(this._input, 26, this._ctx) ) {
			case 1:
				this.enterOuterAlt(_localctx, 1);
				{
				this.state = 264;
				this.commandStatement();
				this.state = 267;
				this._errHandler.sync(this);
				_la = this._input.LA(1);
				if (((((_la - 13)) & ~0x1F) === 0 && ((1 << (_la - 13)) & ((1 << (ScssParser.GT - 13)) | (1 << (ScssParser.LT - 13)) | (1 << (ScssParser.EQEQ - 13)) | (1 << (ScssParser.NOTEQ - 13)))) !== 0)) {
					{
					this.state = 265;
					_la = this._input.LA(1);
					if (!(((((_la - 13)) & ~0x1F) === 0 && ((1 << (_la - 13)) & ((1 << (ScssParser.GT - 13)) | (1 << (ScssParser.LT - 13)) | (1 << (ScssParser.EQEQ - 13)) | (1 << (ScssParser.NOTEQ - 13)))) !== 0))) {
					this._errHandler.recoverInline(this);
					} else {
						if (this._input.LA(1) === Token.EOF) {
							this.matchedEOF = true;
						}

						this._errHandler.reportMatch(this);
						this.consume();
					}
					this.state = 266;
					this.conditions();
					}
				}

				}
				break;

			case 2:
				this.enterOuterAlt(_localctx, 2);
				{
				this.state = 269;
				this.match(ScssParser.LPAREN);
				this.state = 270;
				this.conditions();
				this.state = 271;
				this.match(ScssParser.RPAREN);
				}
				break;
			}
		}
		catch (re) {
			if (re instanceof RecognitionException) {
				_localctx.exception = re;
				this._errHandler.reportError(this, re);
				this._errHandler.recover(this, re);
			} else {
				throw re;
			}
		}
		finally {
			this.exitRule();
		}
		return _localctx;
	}
	// @RuleVersion(0)
	public variableDeclaration(): VariableDeclarationContext {
		let _localctx: VariableDeclarationContext = new VariableDeclarationContext(this._ctx, this.state);
		this.enterRule(_localctx, 42, ScssParser.RULE_variableDeclaration);
		let _la: number;
		try {
			this.enterOuterAlt(_localctx, 1);
			{
			this.state = 275;
			this.variableName();
			this.state = 276;
			this.match(ScssParser.COLON);
			this.state = 277;
			this.values();
			this.state = 279;
			this._errHandler.sync(this);
			_la = this._input.LA(1);
			if (_la === ScssParser.POUND_DEFAULT) {
				{
				this.state = 278;
				this.match(ScssParser.POUND_DEFAULT);
				}
			}

			this.state = 281;
			this.match(ScssParser.SEMI);
			}
		}
		catch (re) {
			if (re instanceof RecognitionException) {
				_localctx.exception = re;
				this._errHandler.reportError(this, re);
				this._errHandler.recover(this, re);
			} else {
				throw re;
			}
		}
		finally {
			this.exitRule();
		}
		return _localctx;
	}
	// @RuleVersion(0)
	public forDeclaration(): ForDeclarationContext {
		let _localctx: ForDeclarationContext = new ForDeclarationContext(this._ctx, this.state);
		this.enterRule(_localctx, 44, ScssParser.RULE_forDeclaration);
		try {
			this.enterOuterAlt(_localctx, 1);
			{
			this.state = 283;
			this.match(ScssParser.AT_FOR);
			this.state = 284;
			this.variableName();
			this.state = 285;
			this.match(ScssParser.FROM);
			this.state = 286;
			this.fromNumber();
			this.state = 287;
			this.match(ScssParser.THROUGH);
			this.state = 288;
			this.throughNumber();
			this.state = 289;
			this.block();
			}
		}
		catch (re) {
			if (re instanceof RecognitionException) {
				_localctx.exception = re;
				this._errHandler.reportError(this, re);
				this._errHandler.recover(this, re);
			} else {
				throw re;
			}
		}
		finally {
			this.exitRule();
		}
		return _localctx;
	}
	// @RuleVersion(0)
	public fromNumber(): FromNumberContext {
		let _localctx: FromNumberContext = new FromNumberContext(this._ctx, this.state);
		this.enterRule(_localctx, 46, ScssParser.RULE_fromNumber);
		try {
			this.enterOuterAlt(_localctx, 1);
			{
			this.state = 291;
			this.match(ScssParser.Number);
			}
		}
		catch (re) {
			if (re instanceof RecognitionException) {
				_localctx.exception = re;
				this._errHandler.reportError(this, re);
				this._errHandler.recover(this, re);
			} else {
				throw re;
			}
		}
		finally {
			this.exitRule();
		}
		return _localctx;
	}
	// @RuleVersion(0)
	public throughNumber(): ThroughNumberContext {
		let _localctx: ThroughNumberContext = new ThroughNumberContext(this._ctx, this.state);
		this.enterRule(_localctx, 48, ScssParser.RULE_throughNumber);
		try {
			this.enterOuterAlt(_localctx, 1);
			{
			this.state = 293;
			this.match(ScssParser.Number);
			}
		}
		catch (re) {
			if (re instanceof RecognitionException) {
				_localctx.exception = re;
				this._errHandler.reportError(this, re);
				this._errHandler.recover(this, re);
			} else {
				throw re;
			}
		}
		finally {
			this.exitRule();
		}
		return _localctx;
	}
	// @RuleVersion(0)
	public whileDeclaration(): WhileDeclarationContext {
		let _localctx: WhileDeclarationContext = new WhileDeclarationContext(this._ctx, this.state);
		this.enterRule(_localctx, 50, ScssParser.RULE_whileDeclaration);
		try {
			this.enterOuterAlt(_localctx, 1);
			{
			this.state = 295;
			this.match(ScssParser.AT_WHILE);
			this.state = 296;
			this.conditions();
			this.state = 297;
			this.block();
			}
		}
		catch (re) {
			if (re instanceof RecognitionException) {
				_localctx.exception = re;
				this._errHandler.reportError(this, re);
				this._errHandler.recover(this, re);
			} else {
				throw re;
			}
		}
		finally {
			this.exitRule();
		}
		return _localctx;
	}
	// @RuleVersion(0)
	public eachDeclaration(): EachDeclarationContext {
		let _localctx: EachDeclarationContext = new EachDeclarationContext(this._ctx, this.state);
		this.enterRule(_localctx, 52, ScssParser.RULE_eachDeclaration);
		let _la: number;
		try {
			this.enterOuterAlt(_localctx, 1);
			{
			this.state = 299;
			this.match(ScssParser.AT_EACH);
			this.state = 300;
			this.variableName();
			this.state = 305;
			this._errHandler.sync(this);
			_la = this._input.LA(1);
			while (_la === ScssParser.COMMA) {
				{
				{
				this.state = 301;
				this.match(ScssParser.COMMA);
				this.state = 302;
				this.variableName();
				}
				}
				this.state = 307;
				this._errHandler.sync(this);
				_la = this._input.LA(1);
			}
			this.state = 308;
			this.match(ScssParser.IN);
			this.state = 309;
			this.eachValueList();
			this.state = 310;
			this.block();
			}
		}
		catch (re) {
			if (re instanceof RecognitionException) {
				_localctx.exception = re;
				this._errHandler.reportError(this, re);
				this._errHandler.recover(this, re);
			} else {
				throw re;
			}
		}
		finally {
			this.exitRule();
		}
		return _localctx;
	}
	// @RuleVersion(0)
	public eachValueList(): EachValueListContext {
		let _localctx: EachValueListContext = new EachValueListContext(this._ctx, this.state);
		this.enterRule(_localctx, 54, ScssParser.RULE_eachValueList);
		let _la: number;
		try {
			this.state = 328;
			this._errHandler.sync(this);
			switch (this._input.LA(1)) {
			case ScssParser.Identifier:
				this.enterOuterAlt(_localctx, 1);
				{
				this.state = 312;
				this.match(ScssParser.Identifier);
				this.state = 317;
				this._errHandler.sync(this);
				_la = this._input.LA(1);
				while (_la === ScssParser.COMMA) {
					{
					{
					this.state = 313;
					this.match(ScssParser.COMMA);
					this.state = 314;
					this.match(ScssParser.Identifier);
					}
					}
					this.state = 319;
					this._errHandler.sync(this);
					_la = this._input.LA(1);
				}
				}
				break;
			case ScssParser.LPAREN:
				this.enterOuterAlt(_localctx, 2);
				{
				this.state = 320;
				this.identifierListOrMap();
				this.state = 325;
				this._errHandler.sync(this);
				_la = this._input.LA(1);
				while (_la === ScssParser.COMMA) {
					{
					{
					this.state = 321;
					this.match(ScssParser.COMMA);
					this.state = 322;
					this.identifierListOrMap();
					}
					}
					this.state = 327;
					this._errHandler.sync(this);
					_la = this._input.LA(1);
				}
				}
				break;
			default:
				throw new NoViableAltException(this);
			}
		}
		catch (re) {
			if (re instanceof RecognitionException) {
				_localctx.exception = re;
				this._errHandler.reportError(this, re);
				this._errHandler.recover(this, re);
			} else {
				throw re;
			}
		}
		finally {
			this.exitRule();
		}
		return _localctx;
	}
	// @RuleVersion(0)
	public identifierListOrMap(): IdentifierListOrMapContext {
		let _localctx: IdentifierListOrMapContext = new IdentifierListOrMapContext(this._ctx, this.state);
		this.enterRule(_localctx, 56, ScssParser.RULE_identifierListOrMap);
		let _la: number;
		try {
			this.enterOuterAlt(_localctx, 1);
			{
			this.state = 330;
			this.match(ScssParser.LPAREN);
			this.state = 331;
			this.identifierValue();
			this.state = 336;
			this._errHandler.sync(this);
			_la = this._input.LA(1);
			while (_la === ScssParser.COMMA) {
				{
				{
				this.state = 332;
				this.match(ScssParser.COMMA);
				this.state = 333;
				this.identifierValue();
				}
				}
				this.state = 338;
				this._errHandler.sync(this);
				_la = this._input.LA(1);
			}
			this.state = 339;
			this.match(ScssParser.RPAREN);
			}
		}
		catch (re) {
			if (re instanceof RecognitionException) {
				_localctx.exception = re;
				this._errHandler.reportError(this, re);
				this._errHandler.recover(this, re);
			} else {
				throw re;
			}
		}
		finally {
			this.exitRule();
		}
		return _localctx;
	}
	// @RuleVersion(0)
	public identifierValue(): IdentifierValueContext {
		let _localctx: IdentifierValueContext = new IdentifierValueContext(this._ctx, this.state);
		this.enterRule(_localctx, 58, ScssParser.RULE_identifierValue);
		let _la: number;
		try {
			this.enterOuterAlt(_localctx, 1);
			{
			this.state = 341;
			this.identifier();
			this.state = 344;
			this._errHandler.sync(this);
			_la = this._input.LA(1);
			if (_la === ScssParser.COLON) {
				{
				this.state = 342;
				this.match(ScssParser.COLON);
				this.state = 343;
				this.values();
				}
			}

			}
		}
		catch (re) {
			if (re instanceof RecognitionException) {
				_localctx.exception = re;
				this._errHandler.reportError(this, re);
				this._errHandler.recover(this, re);
			} else {
				throw re;
			}
		}
		finally {
			this.exitRule();
		}
		return _localctx;
	}
	// @RuleVersion(0)
	public importDeclaration(): ImportDeclarationContext {
		let _localctx: ImportDeclarationContext = new ImportDeclarationContext(this._ctx, this.state);
		this.enterRule(_localctx, 60, ScssParser.RULE_importDeclaration);
		let _la: number;
		try {
			this.enterOuterAlt(_localctx, 1);
			{
			this.state = 346;
			this.match(ScssParser.IMPORT);
			this.state = 347;
			this.referenceUrl();
			this.state = 349;
			this._errHandler.sync(this);
			_la = this._input.LA(1);
			if (_la === ScssParser.Identifier) {
				{
				this.state = 348;
				this.mediaTypes();
				}
			}

			this.state = 351;
			this.match(ScssParser.SEMI);
			}
		}
		catch (re) {
			if (re instanceof RecognitionException) {
				_localctx.exception = re;
				this._errHandler.reportError(this, re);
				this._errHandler.recover(this, re);
			} else {
				throw re;
			}
		}
		finally {
			this.exitRule();
		}
		return _localctx;
	}
	// @RuleVersion(0)
	public referenceUrl(): ReferenceUrlContext {
		let _localctx: ReferenceUrlContext = new ReferenceUrlContext(this._ctx, this.state);
		this.enterRule(_localctx, 62, ScssParser.RULE_referenceUrl);
		try {
			this.state = 357;
			this._errHandler.sync(this);
			switch (this._input.LA(1)) {
			case ScssParser.StringLiteral:
				this.enterOuterAlt(_localctx, 1);
				{
				this.state = 353;
				this.match(ScssParser.StringLiteral);
				}
				break;
			case ScssParser.UrlStart:
				this.enterOuterAlt(_localctx, 2);
				{
				this.state = 354;
				this.match(ScssParser.UrlStart);
				this.state = 355;
				this.match(ScssParser.Url);
				this.state = 356;
				this.match(ScssParser.UrlEnd);
				}
				break;
			default:
				throw new NoViableAltException(this);
			}
		}
		catch (re) {
			if (re instanceof RecognitionException) {
				_localctx.exception = re;
				this._errHandler.reportError(this, re);
				this._errHandler.recover(this, re);
			} else {
				throw re;
			}
		}
		finally {
			this.exitRule();
		}
		return _localctx;
	}
	// @RuleVersion(0)
	public mediaTypes(): MediaTypesContext {
		let _localctx: MediaTypesContext = new MediaTypesContext(this._ctx, this.state);
		this.enterRule(_localctx, 64, ScssParser.RULE_mediaTypes);
		let _la: number;
		try {
			this.enterOuterAlt(_localctx, 1);
			{
			{
			this.state = 359;
			this.match(ScssParser.Identifier);
			this.state = 364;
			this._errHandler.sync(this);
			_la = this._input.LA(1);
			while (_la === ScssParser.COMMA) {
				{
				{
				this.state = 360;
				this.match(ScssParser.COMMA);
				this.state = 361;
				this.match(ScssParser.Identifier);
				}
				}
				this.state = 366;
				this._errHandler.sync(this);
				_la = this._input.LA(1);
			}
			}
			}
		}
		catch (re) {
			if (re instanceof RecognitionException) {
				_localctx.exception = re;
				this._errHandler.reportError(this, re);
				this._errHandler.recover(this, re);
			} else {
				throw re;
			}
		}
		finally {
			this.exitRule();
		}
		return _localctx;
	}
	// @RuleVersion(0)
	public nested(): NestedContext {
		let _localctx: NestedContext = new NestedContext(this._ctx, this.state);
		this.enterRule(_localctx, 66, ScssParser.RULE_nested);
		try {
			this.enterOuterAlt(_localctx, 1);
			{
			this.state = 367;
			this.match(ScssParser.AT);
			this.state = 368;
			this.nest();
			this.state = 369;
			this.selectors();
			this.state = 370;
			this.match(ScssParser.BlockStart);
			this.state = 371;
			this.stylesheet();
			this.state = 372;
			this.match(ScssParser.BlockEnd);
			}
		}
		catch (re) {
			if (re instanceof RecognitionException) {
				_localctx.exception = re;
				this._errHandler.reportError(this, re);
				this._errHandler.recover(this, re);
			} else {
				throw re;
			}
		}
		finally {
			this.exitRule();
		}
		return _localctx;
	}
	// @RuleVersion(0)
	public nest(): NestContext {
		let _localctx: NestContext = new NestContext(this._ctx, this.state);
		this.enterRule(_localctx, 68, ScssParser.RULE_nest);
		let _la: number;
		try {
			let _alt: number;
			this.enterOuterAlt(_localctx, 1);
			{
			this.state = 374;
			_la = this._input.LA(1);
			if (!(_la === ScssParser.AND || _la === ScssParser.Identifier)) {
			this._errHandler.recoverInline(this);
			} else {
				if (this._input.LA(1) === Token.EOF) {
					this.matchedEOF = true;
				}

				this._errHandler.reportMatch(this);
				this.consume();
			}
			this.state = 378;
			this._errHandler.sync(this);
			_alt = this.interpreter.adaptivePredict(this._input, 37, this._ctx);
			while (_alt !== 2 && _alt !== ATN.INVALID_ALT_NUMBER) {
				if (_alt === 1) {
					{
					{
					this.state = 375;
					this.match(ScssParser.Identifier);
					}
					}
				}
				this.state = 380;
				this._errHandler.sync(this);
				_alt = this.interpreter.adaptivePredict(this._input, 37, this._ctx);
			}
			this.state = 384;
			this._errHandler.sync(this);
			_la = this._input.LA(1);
			while (_la === ScssParser.COLON || _la === ScssParser.COLONCOLON) {
				{
				{
				this.state = 381;
				this.pseudo();
				}
				}
				this.state = 386;
				this._errHandler.sync(this);
				_la = this._input.LA(1);
			}
			}
		}
		catch (re) {
			if (re instanceof RecognitionException) {
				_localctx.exception = re;
				this._errHandler.reportError(this, re);
				this._errHandler.recover(this, re);
			} else {
				throw re;
			}
		}
		finally {
			this.exitRule();
		}
		return _localctx;
	}
	// @RuleVersion(0)
	public ruleset(): RulesetContext {
		let _localctx: RulesetContext = new RulesetContext(this._ctx, this.state);
		this.enterRule(_localctx, 70, ScssParser.RULE_ruleset);
		try {
			this.enterOuterAlt(_localctx, 1);
			{
			this.state = 387;
			this.selectors();
			this.state = 388;
			this.block();
			}
		}
		catch (re) {
			if (re instanceof RecognitionException) {
				_localctx.exception = re;
				this._errHandler.reportError(this, re);
				this._errHandler.recover(this, re);
			} else {
				throw re;
			}
		}
		finally {
			this.exitRule();
		}
		return _localctx;
	}
	// @RuleVersion(0)
	public block(): BlockContext {
		let _localctx: BlockContext = new BlockContext(this._ctx, this.state);
		this.enterRule(_localctx, 72, ScssParser.RULE_block);
		let _la: number;
		try {
			let _alt: number;
			this.enterOuterAlt(_localctx, 1);
			{
			this.state = 390;
			this.match(ScssParser.BlockStart);
			this.state = 397;
			this._errHandler.sync(this);
			_alt = this.interpreter.adaptivePredict(this._input, 40, this._ctx);
			while (_alt !== 2 && _alt !== ATN.INVALID_ALT_NUMBER) {
				if (_alt === 1) {
					{
					this.state = 395;
					this._errHandler.sync(this);
					switch ( this.interpreter.adaptivePredict(this._input, 39, this._ctx) ) {
					case 1:
						{
						this.state = 391;
						this.property();
						this.state = 392;
						this.match(ScssParser.SEMI);
						}
						break;

					case 2:
						{
						this.state = 394;
						this.statement();
						}
						break;
					}
					}
				}
				this.state = 399;
				this._errHandler.sync(this);
				_alt = this.interpreter.adaptivePredict(this._input, 40, this._ctx);
			}
			this.state = 401;
			this._errHandler.sync(this);
			_la = this._input.LA(1);
			if (_la === ScssParser.InterpolationStart || _la === ScssParser.Identifier) {
				{
				this.state = 400;
				this.property();
				}
			}

			this.state = 403;
			this.match(ScssParser.BlockEnd);
			}
		}
		catch (re) {
			if (re instanceof RecognitionException) {
				_localctx.exception = re;
				this._errHandler.reportError(this, re);
				this._errHandler.recover(this, re);
			} else {
				throw re;
			}
		}
		finally {
			this.exitRule();
		}
		return _localctx;
	}
	// @RuleVersion(0)
	public selectors(): SelectorsContext {
		let _localctx: SelectorsContext = new SelectorsContext(this._ctx, this.state);
		this.enterRule(_localctx, 74, ScssParser.RULE_selectors);
		let _la: number;
		try {
			this.enterOuterAlt(_localctx, 1);
			{
			this.state = 405;
			this.selector();
			this.state = 410;
			this._errHandler.sync(this);
			_la = this._input.LA(1);
			while (_la === ScssParser.COMMA) {
				{
				{
				this.state = 406;
				this.match(ScssParser.COMMA);
				this.state = 407;
				this.selector();
				}
				}
				this.state = 412;
				this._errHandler.sync(this);
				_la = this._input.LA(1);
			}
			}
		}
		catch (re) {
			if (re instanceof RecognitionException) {
				_localctx.exception = re;
				this._errHandler.reportError(this, re);
				this._errHandler.recover(this, re);
			} else {
				throw re;
			}
		}
		finally {
			this.exitRule();
		}
		return _localctx;
	}
	// @RuleVersion(0)
	public selector(): SelectorContext {
		let _localctx: SelectorContext = new SelectorContext(this._ctx, this.state);
		this.enterRule(_localctx, 76, ScssParser.RULE_selector);
		let _la: number;
		try {
			this.enterOuterAlt(_localctx, 1);
			{
			this.state = 414;
			this._errHandler.sync(this);
			_la = this._input.LA(1);
			do {
				{
				{
				this.state = 413;
				this.element();
				}
				}
				this.state = 416;
				this._errHandler.sync(this);
				_la = this._input.LA(1);
			} while ((((_la) & ~0x1F) === 0 && ((1 << _la) & ((1 << ScssParser.InterpolationStart) | (1 << ScssParser.DOT) | (1 << ScssParser.AND) | (1 << ScssParser.HASH) | (1 << ScssParser.TIMES))) !== 0) || _la === ScssParser.Identifier);
			this.state = 423;
			this._errHandler.sync(this);
			_la = this._input.LA(1);
			while ((((_la) & ~0x1F) === 0 && ((1 << _la) & ((1 << ScssParser.GT) | (1 << ScssParser.TIL) | (1 << ScssParser.PLUS))) !== 0)) {
				{
				{
				this.state = 418;
				this.selectorPrefix();
				this.state = 419;
				this.element();
				}
				}
				this.state = 425;
				this._errHandler.sync(this);
				_la = this._input.LA(1);
			}
			this.state = 429;
			this._errHandler.sync(this);
			_la = this._input.LA(1);
			while (_la === ScssParser.LBRACK) {
				{
				{
				this.state = 426;
				this.attrib();
				}
				}
				this.state = 431;
				this._errHandler.sync(this);
				_la = this._input.LA(1);
			}
			this.state = 433;
			this._errHandler.sync(this);
			_la = this._input.LA(1);
			if (_la === ScssParser.COLON || _la === ScssParser.COLONCOLON) {
				{
				this.state = 432;
				this.pseudo();
				}
			}

			}
		}
		catch (re) {
			if (re instanceof RecognitionException) {
				_localctx.exception = re;
				this._errHandler.reportError(this, re);
				this._errHandler.recover(this, re);
			} else {
				throw re;
			}
		}
		finally {
			this.exitRule();
		}
		return _localctx;
	}
	// @RuleVersion(0)
	public selectorPrefix(): SelectorPrefixContext {
		let _localctx: SelectorPrefixContext = new SelectorPrefixContext(this._ctx, this.state);
		this.enterRule(_localctx, 78, ScssParser.RULE_selectorPrefix);
		let _la: number;
		try {
			this.enterOuterAlt(_localctx, 1);
			{
			this.state = 435;
			_la = this._input.LA(1);
			if (!((((_la) & ~0x1F) === 0 && ((1 << _la) & ((1 << ScssParser.GT) | (1 << ScssParser.TIL) | (1 << ScssParser.PLUS))) !== 0))) {
			this._errHandler.recoverInline(this);
			} else {
				if (this._input.LA(1) === Token.EOF) {
					this.matchedEOF = true;
				}

				this._errHandler.reportMatch(this);
				this.consume();
			}
			}
		}
		catch (re) {
			if (re instanceof RecognitionException) {
				_localctx.exception = re;
				this._errHandler.reportError(this, re);
				this._errHandler.recover(this, re);
			} else {
				throw re;
			}
		}
		finally {
			this.exitRule();
		}
		return _localctx;
	}
	// @RuleVersion(0)
	public element(): ElementContext {
		let _localctx: ElementContext = new ElementContext(this._ctx, this.state);
		this.enterRule(_localctx, 80, ScssParser.RULE_element);
		try {
			this.state = 444;
			this._errHandler.sync(this);
			switch (this._input.LA(1)) {
			case ScssParser.InterpolationStart:
			case ScssParser.Identifier:
				this.enterOuterAlt(_localctx, 1);
				{
				this.state = 437;
				this.identifier();
				}
				break;
			case ScssParser.HASH:
				this.enterOuterAlt(_localctx, 2);
				{
				this.state = 438;
				this.match(ScssParser.HASH);
				this.state = 439;
				this.identifier();
				}
				break;
			case ScssParser.DOT:
				this.enterOuterAlt(_localctx, 3);
				{
				this.state = 440;
				this.match(ScssParser.DOT);
				this.state = 441;
				this.identifier();
				}
				break;
			case ScssParser.AND:
				this.enterOuterAlt(_localctx, 4);
				{
				this.state = 442;
				this.match(ScssParser.AND);
				}
				break;
			case ScssParser.TIMES:
				this.enterOuterAlt(_localctx, 5);
				{
				this.state = 443;
				this.match(ScssParser.TIMES);
				}
				break;
			default:
				throw new NoViableAltException(this);
			}
		}
		catch (re) {
			if (re instanceof RecognitionException) {
				_localctx.exception = re;
				this._errHandler.reportError(this, re);
				this._errHandler.recover(this, re);
			} else {
				throw re;
			}
		}
		finally {
			this.exitRule();
		}
		return _localctx;
	}
	// @RuleVersion(0)
	public pseudo(): PseudoContext {
		let _localctx: PseudoContext = new PseudoContext(this._ctx, this.state);
		this.enterRule(_localctx, 82, ScssParser.RULE_pseudo);
		let _la: number;
		try {
			this.state = 450;
			this._errHandler.sync(this);
			switch ( this.interpreter.adaptivePredict(this._input, 48, this._ctx) ) {
			case 1:
				this.enterOuterAlt(_localctx, 1);
				{
				this.state = 446;
				_la = this._input.LA(1);
				if (!(_la === ScssParser.COLON || _la === ScssParser.COLONCOLON)) {
				this._errHandler.recoverInline(this);
				} else {
					if (this._input.LA(1) === Token.EOF) {
						this.matchedEOF = true;
					}

					this._errHandler.reportMatch(this);
					this.consume();
				}
				this.state = 447;
				this.match(ScssParser.Identifier);
				}
				break;

			case 2:
				this.enterOuterAlt(_localctx, 2);
				{
				this.state = 448;
				_la = this._input.LA(1);
				if (!(_la === ScssParser.COLON || _la === ScssParser.COLONCOLON)) {
				this._errHandler.recoverInline(this);
				} else {
					if (this._input.LA(1) === Token.EOF) {
						this.matchedEOF = true;
					}

					this._errHandler.reportMatch(this);
					this.consume();
				}
				this.state = 449;
				this.functionCall();
				}
				break;
			}
		}
		catch (re) {
			if (re instanceof RecognitionException) {
				_localctx.exception = re;
				this._errHandler.reportError(this, re);
				this._errHandler.recover(this, re);
			} else {
				throw re;
			}
		}
		finally {
			this.exitRule();
		}
		return _localctx;
	}
	// @RuleVersion(0)
	public attrib(): AttribContext {
		let _localctx: AttribContext = new AttribContext(this._ctx, this.state);
		this.enterRule(_localctx, 84, ScssParser.RULE_attrib);
		let _la: number;
		try {
			this.enterOuterAlt(_localctx, 1);
			{
			this.state = 452;
			this.match(ScssParser.LBRACK);
			this.state = 453;
			this.match(ScssParser.Identifier);
			this.state = 457;
			this._errHandler.sync(this);
			_la = this._input.LA(1);
			if (((((_la - 33)) & ~0x1F) === 0 && ((1 << (_la - 33)) & ((1 << (ScssParser.EQ - 33)) | (1 << (ScssParser.PIPE_EQ - 33)) | (1 << (ScssParser.TILD_EQ - 33)))) !== 0)) {
				{
				this.state = 454;
				this.attribRelate();
				this.state = 455;
				_la = this._input.LA(1);
				if (!(_la === ScssParser.Identifier || _la === ScssParser.StringLiteral)) {
				this._errHandler.recoverInline(this);
				} else {
					if (this._input.LA(1) === Token.EOF) {
						this.matchedEOF = true;
					}

					this._errHandler.reportMatch(this);
					this.consume();
				}
				}
			}

			this.state = 459;
			this.match(ScssParser.RBRACK);
			}
		}
		catch (re) {
			if (re instanceof RecognitionException) {
				_localctx.exception = re;
				this._errHandler.reportError(this, re);
				this._errHandler.recover(this, re);
			} else {
				throw re;
			}
		}
		finally {
			this.exitRule();
		}
		return _localctx;
	}
	// @RuleVersion(0)
	public attribRelate(): AttribRelateContext {
		let _localctx: AttribRelateContext = new AttribRelateContext(this._ctx, this.state);
		this.enterRule(_localctx, 86, ScssParser.RULE_attribRelate);
		let _la: number;
		try {
			this.enterOuterAlt(_localctx, 1);
			{
			this.state = 461;
			_la = this._input.LA(1);
			if (!(((((_la - 33)) & ~0x1F) === 0 && ((1 << (_la - 33)) & ((1 << (ScssParser.EQ - 33)) | (1 << (ScssParser.PIPE_EQ - 33)) | (1 << (ScssParser.TILD_EQ - 33)))) !== 0))) {
			this._errHandler.recoverInline(this);
			} else {
				if (this._input.LA(1) === Token.EOF) {
					this.matchedEOF = true;
				}

				this._errHandler.reportMatch(this);
				this.consume();
			}
			}
		}
		catch (re) {
			if (re instanceof RecognitionException) {
				_localctx.exception = re;
				this._errHandler.reportError(this, re);
				this._errHandler.recover(this, re);
			} else {
				throw re;
			}
		}
		finally {
			this.exitRule();
		}
		return _localctx;
	}
	// @RuleVersion(0)
	public identifier(): IdentifierContext {
		let _localctx: IdentifierContext = new IdentifierContext(this._ctx, this.state);
		this.enterRule(_localctx, 88, ScssParser.RULE_identifier);
		let _la: number;
		try {
			this.state = 479;
			this._errHandler.sync(this);
			switch (this._input.LA(1)) {
			case ScssParser.Identifier:
				this.enterOuterAlt(_localctx, 1);
				{
				this.state = 463;
				this.match(ScssParser.Identifier);
				this.state = 467;
				this._errHandler.sync(this);
				_la = this._input.LA(1);
				while (_la === ScssParser.InterpolationStartAfter || _la === ScssParser.IdentifierAfter) {
					{
					{
					this.state = 464;
					this.identifierPart();
					}
					}
					this.state = 469;
					this._errHandler.sync(this);
					_la = this._input.LA(1);
				}
				}
				break;
			case ScssParser.InterpolationStart:
				this.enterOuterAlt(_localctx, 2);
				{
				this.state = 470;
				this.match(ScssParser.InterpolationStart);
				this.state = 471;
				this.identifierVariableName();
				this.state = 472;
				this.match(ScssParser.BlockEnd);
				this.state = 476;
				this._errHandler.sync(this);
				_la = this._input.LA(1);
				while (_la === ScssParser.InterpolationStartAfter || _la === ScssParser.IdentifierAfter) {
					{
					{
					this.state = 473;
					this.identifierPart();
					}
					}
					this.state = 478;
					this._errHandler.sync(this);
					_la = this._input.LA(1);
				}
				}
				break;
			default:
				throw new NoViableAltException(this);
			}
		}
		catch (re) {
			if (re instanceof RecognitionException) {
				_localctx.exception = re;
				this._errHandler.reportError(this, re);
				this._errHandler.recover(this, re);
			} else {
				throw re;
			}
		}
		finally {
			this.exitRule();
		}
		return _localctx;
	}
	// @RuleVersion(0)
	public identifierPart(): IdentifierPartContext {
		let _localctx: IdentifierPartContext = new IdentifierPartContext(this._ctx, this.state);
		this.enterRule(_localctx, 90, ScssParser.RULE_identifierPart);
		try {
			this.state = 486;
			this._errHandler.sync(this);
			switch (this._input.LA(1)) {
			case ScssParser.InterpolationStartAfter:
				this.enterOuterAlt(_localctx, 1);
				{
				this.state = 481;
				this.match(ScssParser.InterpolationStartAfter);
				this.state = 482;
				this.identifierVariableName();
				this.state = 483;
				this.match(ScssParser.BlockEnd);
				}
				break;
			case ScssParser.IdentifierAfter:
				this.enterOuterAlt(_localctx, 2);
				{
				this.state = 485;
				this.match(ScssParser.IdentifierAfter);
				}
				break;
			default:
				throw new NoViableAltException(this);
			}
		}
		catch (re) {
			if (re instanceof RecognitionException) {
				_localctx.exception = re;
				this._errHandler.reportError(this, re);
				this._errHandler.recover(this, re);
			} else {
				throw re;
			}
		}
		finally {
			this.exitRule();
		}
		return _localctx;
	}
	// @RuleVersion(0)
	public identifierVariableName(): IdentifierVariableNameContext {
		let _localctx: IdentifierVariableNameContext = new IdentifierVariableNameContext(this._ctx, this.state);
		this.enterRule(_localctx, 92, ScssParser.RULE_identifierVariableName);
		let _la: number;
		try {
			this.enterOuterAlt(_localctx, 1);
			{
			this.state = 488;
			this.match(ScssParser.DOLLAR);
			this.state = 489;
			_la = this._input.LA(1);
			if (!(_la === ScssParser.Identifier || _la === ScssParser.IdentifierAfter)) {
			this._errHandler.recoverInline(this);
			} else {
				if (this._input.LA(1) === Token.EOF) {
					this.matchedEOF = true;
				}

				this._errHandler.reportMatch(this);
				this.consume();
			}
			}
		}
		catch (re) {
			if (re instanceof RecognitionException) {
				_localctx.exception = re;
				this._errHandler.reportError(this, re);
				this._errHandler.recover(this, re);
			} else {
				throw re;
			}
		}
		finally {
			this.exitRule();
		}
		return _localctx;
	}
	// @RuleVersion(0)
	public property(): PropertyContext {
		let _localctx: PropertyContext = new PropertyContext(this._ctx, this.state);
		this.enterRule(_localctx, 94, ScssParser.RULE_property);
		try {
			this.enterOuterAlt(_localctx, 1);
			{
			this.state = 491;
			this.identifier();
			this.state = 492;
			this.match(ScssParser.COLON);
			this.state = 493;
			this.values();
			}
		}
		catch (re) {
			if (re instanceof RecognitionException) {
				_localctx.exception = re;
				this._errHandler.reportError(this, re);
				this._errHandler.recover(this, re);
			} else {
				throw re;
			}
		}
		finally {
			this.exitRule();
		}
		return _localctx;
	}
	// @RuleVersion(0)
	public values(): ValuesContext {
		let _localctx: ValuesContext = new ValuesContext(this._ctx, this.state);
		this.enterRule(_localctx, 96, ScssParser.RULE_values);
		try {
			let _alt: number;
			this.enterOuterAlt(_localctx, 1);
			{
			this.state = 495;
			this.commandStatement();
			this.state = 500;
			this._errHandler.sync(this);
			_alt = this.interpreter.adaptivePredict(this._input, 54, this._ctx);
			while (_alt !== 2 && _alt !== ATN.INVALID_ALT_NUMBER) {
				if (_alt === 1) {
					{
					{
					this.state = 496;
					this.match(ScssParser.COMMA);
					this.state = 497;
					this.commandStatement();
					}
					}
				}
				this.state = 502;
				this._errHandler.sync(this);
				_alt = this.interpreter.adaptivePredict(this._input, 54, this._ctx);
			}
			}
		}
		catch (re) {
			if (re instanceof RecognitionException) {
				_localctx.exception = re;
				this._errHandler.reportError(this, re);
				this._errHandler.recover(this, re);
			} else {
				throw re;
			}
		}
		finally {
			this.exitRule();
		}
		return _localctx;
	}
	// @RuleVersion(0)
	public url(): UrlContext {
		let _localctx: UrlContext = new UrlContext(this._ctx, this.state);
		this.enterRule(_localctx, 98, ScssParser.RULE_url);
		try {
			this.enterOuterAlt(_localctx, 1);
			{
			this.state = 503;
			this.match(ScssParser.UrlStart);
			this.state = 504;
			this.match(ScssParser.Url);
			this.state = 505;
			this.match(ScssParser.UrlEnd);
			}
		}
		catch (re) {
			if (re instanceof RecognitionException) {
				_localctx.exception = re;
				this._errHandler.reportError(this, re);
				this._errHandler.recover(this, re);
			} else {
				throw re;
			}
		}
		finally {
			this.exitRule();
		}
		return _localctx;
	}
	// @RuleVersion(0)
	public measurement(): MeasurementContext {
		let _localctx: MeasurementContext = new MeasurementContext(this._ctx, this.state);
		this.enterRule(_localctx, 100, ScssParser.RULE_measurement);
		let _la: number;
		try {
			this.enterOuterAlt(_localctx, 1);
			{
			this.state = 507;
			this.match(ScssParser.Number);
			this.state = 509;
			this._errHandler.sync(this);
			_la = this._input.LA(1);
			if (_la === ScssParser.Unit) {
				{
				this.state = 508;
				this.match(ScssParser.Unit);
				}
			}

			}
		}
		catch (re) {
			if (re instanceof RecognitionException) {
				_localctx.exception = re;
				this._errHandler.reportError(this, re);
				this._errHandler.recover(this, re);
			} else {
				throw re;
			}
		}
		finally {
			this.exitRule();
		}
		return _localctx;
	}
	// @RuleVersion(0)
	public functionCall(): FunctionCallContext {
		let _localctx: FunctionCallContext = new FunctionCallContext(this._ctx, this.state);
		this.enterRule(_localctx, 102, ScssParser.RULE_functionCall);
		let _la: number;
		try {
			this.enterOuterAlt(_localctx, 1);
			{
			this.state = 511;
			this.match(ScssParser.Identifier);
			this.state = 512;
			this.match(ScssParser.LPAREN);
			this.state = 514;
			this._errHandler.sync(this);
			_la = this._input.LA(1);
			if ((((_la) & ~0x1F) === 0 && ((1 << _la) & ((1 << ScssParser.NULL) | (1 << ScssParser.InterpolationStart) | (1 << ScssParser.LPAREN) | (1 << ScssParser.DOLLAR) | (1 << ScssParser.UrlStart))) !== 0) || ((((_la - 50)) & ~0x1F) === 0 && ((1 << (_la - 50)) & ((1 << (ScssParser.Identifier - 50)) | (1 << (ScssParser.StringLiteral - 50)) | (1 << (ScssParser.Number - 50)) | (1 << (ScssParser.Color - 50)))) !== 0)) {
				{
				this.state = 513;
				this.values();
				}
			}

			this.state = 516;
			this.match(ScssParser.RPAREN);
			}
		}
		catch (re) {
			if (re instanceof RecognitionException) {
				_localctx.exception = re;
				this._errHandler.reportError(this, re);
				this._errHandler.recover(this, re);
			} else {
				throw re;
			}
		}
		finally {
			this.exitRule();
		}
		return _localctx;
	}

	public static readonly _serializedATN: string =
		"\x03\uC91D\uCABA\u058D\uAFBA\u4F53\u0607\uEA8B\uC241\x03?\u0209\x04\x02" +
		"\t\x02\x04\x03\t\x03\x04\x04\t\x04\x04\x05\t\x05\x04\x06\t\x06\x04\x07" +
		"\t\x07\x04\b\t\b\x04\t\t\t\x04\n\t\n\x04\v\t\v\x04\f\t\f\x04\r\t\r\x04" +
		"\x0E\t\x0E\x04\x0F\t\x0F\x04\x10\t\x10\x04\x11\t\x11\x04\x12\t\x12\x04" +
		"\x13\t\x13\x04\x14\t\x14\x04\x15\t\x15\x04\x16\t\x16\x04\x17\t\x17\x04" +
		"\x18\t\x18\x04\x19\t\x19\x04\x1A\t\x1A\x04\x1B\t\x1B\x04\x1C\t\x1C\x04" +
		"\x1D\t\x1D\x04\x1E\t\x1E\x04\x1F\t\x1F\x04 \t \x04!\t!\x04\"\t\"\x04#" +
		"\t#\x04$\t$\x04%\t%\x04&\t&\x04\'\t\'\x04(\t(\x04)\t)\x04*\t*\x04+\t+" +
		"\x04,\t,\x04-\t-\x04.\t.\x04/\t/\x040\t0\x041\t1\x042\t2\x043\t3\x044" +
		"\t4\x045\t5\x03\x02\x07\x02l\n\x02\f\x02\x0E\x02o\v\x02\x03\x03\x03\x03" +
		"\x03\x03\x03\x03\x03\x03\x03\x03\x03\x03\x03\x03\x03\x03\x03\x03\x03\x03" +
		"\x05\x03|\n\x03\x03\x04\x03\x04\x03\x04\x07\x04\x81\n\x04\f\x04\x0E\x04" +
		"\x84\v\x04\x03\x04\x05\x04\x87\n\x04\x03\x05\x03\x05\x05\x05\x8B\n\x05" +
		"\x03\x06\x03\x06\x03\x06\x03\x07\x03\x07\x06\x07\x92\n\x07\r\x07\x0E\x07" +
		"\x93\x03\b\x03\b\x03\b\x03\b\x05\b\x9A\n\b\x03\b\x05\b\x9D\n\b\x03\b\x03" +
		"\b\x03\t\x03\t\x03\t\x03\t\x03\t\x05\t\xA6\n\t\x03\t\x03\t\x05\t\xAA\n" +
		"\t\x05\t\xAC\n\t\x03\t\x05\t\xAF\n\t\x05\t\xB1\n\t\x03\n\x03\n\x03\n\x03" +
		"\n\x05\n\xB7\n\n\x03\n\x03\n\x03\n\x05\n\xBC\n\n\x03\n\x03\n\x03\v\x07" +
		"\v\xC1\n\v\f\v\x0E\v\xC4\v\v\x03\v\x03\v\x03\f\x03\f\x03\f\x03\f\x03\r" +
		"\x03\r\x03\r\x03\r\x05\r\xD0\n\r\x03\x0E\x06\x0E\xD3\n\x0E\r\x0E\x0E\x0E" +
		"\xD4\x03\x0E\x03\x0E\x03\x0E\x03\x0E\x05\x0E\xDB\n\x0E\x03\x0E\x05\x0E" +
		"\xDE\n\x0E\x03\x0F\x03\x0F\x03\x10\x03\x10\x03\x10\x03\x11\x03\x11\x03" +
		"\x11\x03\x11\x03\x11\x03\x11\x03\x11\x03\x11\x05\x11\xED\n\x11\x03\x12" +
		"\x03\x12\x03\x12\x03\x12\x07\x12\xF3\n\x12\f\x12\x0E\x12\xF6\v\x12\x03" +
		"\x12\x05\x12\xF9\n\x12\x03\x13\x03\x13\x03\x13\x03\x13\x03\x13\x03\x14" +
		"\x03\x14\x03\x14\x03\x15\x03\x15\x03\x15\x05\x15\u0106\n\x15\x03\x15\x05" +
		"\x15\u0109\n\x15\x03\x16\x03\x16\x03\x16\x05\x16\u010E\n\x16\x03\x16\x03" +
		"\x16\x03\x16\x03\x16\x05\x16\u0114\n\x16\x03\x17\x03\x17\x03\x17\x03\x17" +
		"\x05\x17\u011A\n\x17\x03\x17\x03\x17\x03\x18\x03\x18\x03\x18\x03\x18\x03" +
		"\x18\x03\x18\x03\x18\x03\x18\x03\x19\x03\x19\x03\x1A\x03\x1A\x03\x1B\x03" +
		"\x1B\x03\x1B\x03\x1B\x03\x1C\x03\x1C\x03\x1C\x03\x1C\x07\x1C\u0132\n\x1C" +
		"\f\x1C\x0E\x1C\u0135\v\x1C\x03\x1C\x03\x1C\x03\x1C\x03\x1C\x03\x1D\x03" +
		"\x1D\x03\x1D\x07\x1D\u013E\n\x1D\f\x1D\x0E\x1D\u0141\v\x1D\x03\x1D\x03" +
		"\x1D\x03\x1D\x07\x1D\u0146\n\x1D\f\x1D\x0E\x1D\u0149\v\x1D\x05\x1D\u014B" +
		"\n\x1D\x03\x1E\x03\x1E\x03\x1E\x03\x1E\x07\x1E\u0151\n\x1E\f\x1E\x0E\x1E" +
		"\u0154\v\x1E\x03\x1E\x03\x1E\x03\x1F\x03\x1F\x03\x1F\x05\x1F\u015B\n\x1F" +
		"\x03 \x03 \x03 \x05 \u0160\n \x03 \x03 \x03!\x03!\x03!\x03!\x05!\u0168" +
		"\n!\x03\"\x03\"\x03\"\x07\"\u016D\n\"\f\"\x0E\"\u0170\v\"\x03#\x03#\x03" +
		"#\x03#\x03#\x03#\x03#\x03$\x03$\x07$\u017B\n$\f$\x0E$\u017E\v$\x03$\x07" +
		"$\u0181\n$\f$\x0E$\u0184\v$\x03%\x03%\x03%\x03&\x03&\x03&\x03&\x03&\x07" +
		"&\u018E\n&\f&\x0E&\u0191\v&\x03&\x05&\u0194\n&\x03&\x03&\x03\'\x03\'\x03" +
		"\'\x07\'\u019B\n\'\f\'\x0E\'\u019E\v\'\x03(\x06(\u01A1\n(\r(\x0E(\u01A2" +
		"\x03(\x03(\x03(\x07(\u01A8\n(\f(\x0E(\u01AB\v(\x03(\x07(\u01AE\n(\f(\x0E" +
		"(\u01B1\v(\x03(\x05(\u01B4\n(\x03)\x03)\x03*\x03*\x03*\x03*\x03*\x03*" +
		"\x03*\x05*\u01BF\n*\x03+\x03+\x03+\x03+\x05+\u01C5\n+\x03,\x03,\x03,\x03" +
		",\x03,\x05,\u01CC\n,\x03,\x03,\x03-\x03-\x03.\x03.\x07.\u01D4\n.\f.\x0E" +
		".\u01D7\v.\x03.\x03.\x03.\x03.\x07.\u01DD\n.\f.\x0E.\u01E0\v.\x05.\u01E2" +
		"\n.\x03/\x03/\x03/\x03/\x03/\x05/\u01E9\n/\x030\x030\x030\x031\x031\x03" +
		"1\x031\x032\x032\x032\x072\u01F5\n2\f2\x0E2\u01F8\v2\x033\x033\x033\x03" +
		"3\x034\x034\x054\u0200\n4\x035\x035\x035\x055\u0205\n5\x035\x035\x035" +
		"\x02\x02\x026\x02\x02\x04\x02\x06\x02\b\x02\n\x02\f\x02\x0E\x02\x10\x02" +
		"\x12\x02\x14\x02\x16\x02\x18\x02\x1A\x02\x1C\x02\x1E\x02 \x02\"\x02$\x02" +
		"&\x02(\x02*\x02,\x02.\x020\x022\x024\x026\x028\x02:\x02<\x02>\x02@\x02" +
		"B\x02D\x02F\x02H\x02J\x02L\x02N\x02P\x02R\x02T\x02V\x02X\x02Z\x02\\\x02" +
		"^\x02`\x02b\x02d\x02f\x02h\x02\x02\n\x03\x02\x1B\x1F\x05\x02\x0F\x0F\x11" +
		"\x11!\"\x04\x02\x18\x1844\x04\x02\x0F\x10\x1B\x1B\x04\x02\x12\x12\x1A" +
		"\x1A\x03\x0245\x03\x02#%\x04\x0244??\x02\u021F\x02m\x03\x02\x02\x02\x04" +
		"{\x03\x02\x02\x02\x06}\x03\x02\x02\x02\b\x88\x03\x02\x02\x02\n\x8C\x03" +
		"\x02\x02\x02\f\x8F\x03\x02\x02\x02\x0E\x95\x03\x02\x02\x02\x10\xA0\x03" +
		"\x02\x02\x02\x12\xB2\x03\x02\x02\x02\x14\xC2\x03\x02\x02\x02\x16\xC7\x03" +
		"\x02\x02\x02\x18\xCF\x03\x02\x02\x02\x1A\xDA\x03\x02\x02\x02\x1C\xDF\x03" +
		"\x02\x02\x02\x1E\xE1\x03\x02\x02\x02 \xEC\x03\x02\x02\x02\"\xEE\x03\x02" +
		"\x02\x02$\xFA\x03\x02\x02\x02&\xFF\x03\x02\x02\x02(\u0108\x03\x02\x02" +
		"\x02*\u0113\x03\x02\x02\x02,\u0115\x03\x02\x02\x02.\u011D\x03\x02\x02" +
		"\x020\u0125\x03\x02\x02\x022\u0127\x03\x02\x02\x024\u0129\x03\x02\x02" +
		"\x026\u012D\x03\x02\x02\x028\u014A\x03\x02\x02\x02:\u014C\x03\x02\x02" +
		"\x02<\u0157\x03\x02\x02\x02>\u015C\x03\x02\x02\x02@\u0167\x03\x02\x02" +
		"\x02B\u0169\x03\x02\x02\x02D\u0171\x03\x02\x02\x02F\u0178\x03\x02\x02" +
		"\x02H\u0185\x03\x02\x02\x02J\u0188\x03\x02\x02\x02L\u0197\x03\x02\x02" +
		"\x02N\u01A0\x03\x02\x02\x02P\u01B5\x03\x02\x02\x02R\u01BE\x03\x02\x02" +
		"\x02T\u01C4\x03\x02\x02\x02V\u01C6\x03\x02\x02\x02X\u01CF\x03\x02\x02" +
		"\x02Z\u01E1\x03\x02\x02\x02\\\u01E8\x03\x02\x02\x02^\u01EA\x03\x02\x02" +
		"\x02`\u01ED\x03\x02\x02\x02b\u01F1\x03\x02\x02\x02d\u01F9\x03\x02\x02" +
		"\x02f\u01FD\x03\x02\x02\x02h\u0201\x03\x02\x02\x02jl\x05\x04\x03\x02k" +
		"j\x03\x02\x02\x02lo\x03\x02\x02\x02mk\x03\x02\x02\x02mn\x03\x02\x02\x02" +
		"n\x03\x03\x02\x02\x02om\x03\x02\x02\x02p|\x05> \x02q|\x05D#\x02r|\x05" +
		"H%\x02s|\x05\x0E\b\x02t|\x05\x12\n\x02u|\x05,\x17\x02v|\x05\x10\t\x02" +
		"w|\x05\"\x12\x02x|\x05.\x18\x02y|\x054\x1B\x02z|\x056\x1C\x02{p\x03\x02" +
		"\x02\x02{q\x03\x02\x02\x02{r\x03\x02\x02\x02{s\x03\x02\x02\x02{t\x03\x02" +
		"\x02\x02{u\x03\x02\x02\x02{v\x03\x02\x02\x02{w\x03\x02\x02\x02{x\x03\x02" +
		"\x02\x02{y\x03\x02\x02\x02{z\x03\x02\x02\x02|\x05\x03\x02\x02\x02}\x82" +
		"\x05\b\x05\x02~\x7F\x07\x14\x02\x02\x7F\x81\x05\b\x05\x02\x80~\x03\x02" +
		"\x02\x02\x81\x84\x03\x02\x02\x02\x82\x80\x03\x02\x02\x02\x82\x83\x03\x02" +
		"\x02\x02\x83\x86\x03\x02\x02\x02\x84\x82\x03\x02\x02\x02\x85\x87\x07\x07" +
		"\x02\x02\x86\x85\x03\x02\x02\x02\x86\x87\x03\x02\x02\x02\x87\x07\x03\x02" +
		"\x02\x02\x88\x8A\x05\n\x06\x02\x89\x8B\x05\f\x07\x02\x8A\x89\x03\x02\x02" +
		"\x02\x8A\x8B\x03\x02\x02\x02\x8B\t\x03\x02\x02\x02\x8C\x8D\x07\x16\x02" +
		"\x02\x8D\x8E\x074\x02\x02\x8E\v\x03\x02\x02\x02\x8F\x91\x07\x12\x02\x02" +
		"\x90\x92\x05 \x11\x02\x91\x90\x03\x02\x02\x02\x92\x93\x03\x02\x02\x02" +
		"\x93\x91\x03\x02\x02\x02\x93\x94\x03\x02\x02\x02\x94\r\x03\x02\x02\x02" +
		"\x95\x96\x07&\x02\x02\x96\x9C\x074\x02\x02\x97\x99\x07\t\x02\x02\x98\x9A" +
		"\x05\x06\x04\x02\x99\x98\x03\x02\x02\x02\x99\x9A\x03\x02\x02\x02\x9A\x9B" +
		"\x03\x02\x02\x02\x9B\x9D\x07\n\x02\x02\x9C\x97\x03\x02\x02\x02\x9C\x9D" +
		"\x03\x02\x02\x02\x9D\x9E\x03\x02\x02\x02\x9E\x9F\x05J&\x02\x9F\x0F\x03" +
		"\x02\x02\x02\xA0\xA1\x07.\x02\x02\xA1\xB0\x074\x02\x02\xA2\xB1\x07\x13" +
		"\x02\x02\xA3\xA5\x07\t\x02\x02\xA4\xA6\x05b2\x02\xA5\xA4\x03\x02\x02\x02" +
		"\xA5\xA6\x03\x02\x02\x02\xA6\xA7\x03\x02\x02\x02\xA7\xA9\x07\n\x02\x02" +
		"\xA8\xAA\x07\x13\x02\x02\xA9\xA8\x03\x02\x02\x02\xA9\xAA\x03\x02\x02\x02" +
		"\xAA\xAC\x03\x02\x02\x02\xAB\xA3\x03\x02\x02\x02\xAB\xAC\x03\x02\x02\x02" +
		"\xAC\xAE\x03\x02\x02\x02\xAD\xAF\x05J&\x02\xAE\xAD\x03\x02\x02\x02\xAE" +
		"\xAF\x03\x02\x02\x02\xAF\xB1\x03\x02\x02\x02\xB0\xA2\x03\x02\x02\x02\xB0" +
		"\xAB\x03\x02\x02\x02\xB1\x11\x03\x02\x02\x02\xB2\xB3\x07\'\x02\x02\xB3" +
		"\xB4\x074\x02\x02\xB4\xB6\x07\t\x02\x02\xB5\xB7\x05\x06\x04\x02\xB6\xB5" +
		"\x03\x02\x02\x02\xB6\xB7\x03\x02\x02\x02\xB7\xB8\x03\x02\x02\x02\xB8\xB9" +
		"\x07\n\x02\x02\xB9\xBB\x07\v\x02\x02\xBA\xBC\x05\x14\v\x02\xBB\xBA\x03" +
		"\x02\x02\x02\xBB\xBC\x03\x02\x02\x02\xBC\xBD\x03\x02\x02\x02\xBD\xBE\x07" +
		"\f\x02\x02\xBE\x13\x03\x02\x02\x02\xBF\xC1\x05\x18\r\x02\xC0\xBF\x03\x02" +
		"\x02\x02\xC1\xC4\x03\x02\x02\x02\xC2\xC0\x03\x02\x02\x02\xC2\xC3\x03\x02" +
		"\x02\x02\xC3\xC5\x03\x02\x02\x02\xC4\xC2\x03\x02\x02\x02\xC5\xC6\x05\x16" +
		"\f\x02\xC6\x15\x03\x02\x02\x02\xC7\xC8\x070\x02\x02\xC8\xC9\x05\x1A\x0E" +
		"\x02\xC9\xCA\x07\x13\x02\x02\xCA\x17\x03\x02\x02\x02\xCB\xCC\x05\x1A\x0E" +
		"\x02\xCC\xCD\x07\x13\x02\x02\xCD\xD0\x03\x02\x02\x02\xCE\xD0\x05\x04\x03" +
		"\x02\xCF\xCB\x03\x02\x02\x02\xCF\xCE\x03\x02\x02\x02\xD0\x19\x03\x02\x02" +
		"\x02\xD1\xD3\x05 \x11\x02\xD2\xD1\x03\x02\x02\x02\xD3\xD4\x03\x02\x02" +
		"\x02\xD4\xD2\x03\x02\x02\x02\xD4\xD5\x03\x02\x02\x02\xD5\xDB\x03\x02\x02" +
		"\x02\xD6\xD7\x07\t\x02\x02\xD7\xD8\x05\x1A\x0E\x02\xD8\xD9\x07\n\x02\x02" +
		"\xD9\xDB\x03\x02\x02\x02\xDA\xD2\x03\x02\x02\x02\xDA\xD6\x03\x02\x02\x02" +
		"\xDB\xDD\x03\x02\x02\x02\xDC\xDE\x05\x1E\x10\x02\xDD\xDC\x03\x02\x02\x02" +
		"\xDD\xDE\x03\x02\x02\x02\xDE\x1B\x03\x02\x02\x02\xDF\xE0\t\x02\x02\x02" +
		"\xE0\x1D\x03\x02\x02\x02\xE1\xE2\x05\x1C\x0F\x02\xE2\xE3\x05\x1A\x0E\x02" +
		"\xE3\x1F\x03\x02\x02\x02\xE4\xED\x05f4\x02\xE5\xED\x05Z.\x02\xE6\xED\x07" +
		"7\x02\x02\xE7\xED\x075\x02\x02\xE8\xED\x07\x03\x02\x02\xE9\xED\x05d3\x02" +
		"\xEA\xED\x05\n\x06\x02\xEB\xED\x05h5\x02\xEC\xE4\x03\x02\x02\x02\xEC\xE5" +
		"\x03\x02\x02\x02\xEC\xE6\x03\x02\x02\x02\xEC\xE7\x03\x02\x02\x02\xEC\xE8" +
		"\x03\x02\x02\x02\xEC\xE9\x03\x02\x02\x02\xEC\xEA\x03\x02\x02\x02\xEC\xEB" +
		"\x03\x02\x02\x02\xED!\x03\x02\x02\x02\xEE\xEF\x07*\x02\x02\xEF\xF0\x05" +
		"(\x15\x02\xF0\xF4\x05J&\x02\xF1\xF3\x05$\x13\x02\xF2\xF1\x03\x02\x02\x02" +
		"\xF3\xF6\x03\x02\x02\x02\xF4\xF2\x03\x02\x02\x02\xF4\xF5\x03\x02\x02\x02" +
		"\xF5\xF8\x03\x02\x02\x02\xF6\xF4\x03\x02\x02\x02\xF7\xF9\x05&\x14\x02" +
		"\xF8\xF7\x03\x02\x02\x02\xF8\xF9\x03\x02\x02\x02\xF9#\x03\x02\x02\x02" +
		"\xFA\xFB\x07(\x02\x02\xFB\xFC\x07)\x02\x02\xFC\xFD\x05(\x15\x02\xFD\xFE" +
		"\x05J&\x02\xFE%\x03\x02\x02\x02\xFF\u0100\x07(\x02\x02\u0100\u0101\x05" +
		"J&\x02\u0101\'\x03\x02\x02\x02\u0102\u0105\x05*\x16\x02\u0103\u0104\x07" +
		"\x06\x02\x02\u0104\u0106\x05(\x15\x02\u0105\u0103\x03\x02\x02\x02\u0105" +
		"\u0106\x03\x02\x02\x02\u0106\u0109\x03\x02\x02\x02\u0107\u0109\x07\x03" +
		"\x02\x02\u0108\u0102\x03\x02\x02\x02\u0108\u0107\x03\x02\x02\x02\u0109" +
		")\x03\x02\x02\x02\u010A\u010D\x05\x1A\x0E\x02\u010B\u010C\t\x03\x02\x02" +
		"\u010C\u010E\x05(\x15\x02\u010D\u010B\x03\x02\x02\x02\u010D\u010E\x03" +
		"\x02\x02\x02\u010E\u0114\x03\x02\x02\x02\u010F\u0110\x07\t\x02\x02\u0110" +
		"\u0111\x05(\x15\x02\u0111\u0112\x07\n\x02\x02\u0112\u0114\x03\x02\x02" +
		"\x02\u0113\u010A\x03\x02\x02\x02\u0113\u010F\x03\x02\x02\x02\u0114+\x03" +
		"\x02\x02\x02\u0115\u0116\x05\n\x06\x02\u0116\u0117\x07\x12\x02\x02\u0117" +
		"\u0119\x05b2\x02\u0118\u011A\x073\x02\x02\u0119\u0118\x03\x02\x02\x02" +
		"\u0119\u011A\x03\x02\x02\x02\u011A\u011B\x03\x02\x02\x02\u011B\u011C\x07" +
		"\x13\x02\x02\u011C-\x03\x02\x02\x02\u011D\u011E\x07+\x02\x02\u011E\u011F" +
		"\x05\n\x06\x02\u011F\u0120\x071\x02\x02\u0120\u0121\x050\x19\x02\u0121" +
		"\u0122\x072\x02\x02\u0122\u0123\x052\x1A\x02\u0123\u0124\x05J&\x02\u0124" +
		"/\x03\x02\x02\x02\u0125\u0126\x076\x02\x02\u01261\x03\x02\x02\x02\u0127" +
		"\u0128\x076\x02\x02\u01283\x03\x02\x02\x02\u0129\u012A\x07,\x02\x02\u012A" +
		"\u012B\x05(\x15\x02\u012B\u012C\x05J&\x02\u012C5\x03\x02\x02\x02\u012D" +
		"\u012E\x07-\x02\x02\u012E\u0133\x05\n\x06\x02\u012F\u0130\x07\x14\x02" +
		"\x02\u0130\u0132\x05\n\x06\x02\u0131\u012F\x03\x02\x02\x02\u0132\u0135" +
		"\x03\x02\x02\x02\u0133\u0131\x03\x02\x02\x02\u0133\u0134\x03\x02\x02\x02" +
		"\u0134\u0136\x03\x02\x02\x02\u0135\u0133\x03\x02\x02\x02\u0136\u0137\x07" +
		"\x04\x02\x02\u0137\u0138\x058\x1D\x02\u0138\u0139\x05J&\x02\u01397\x03" +
		"\x02\x02\x02\u013A\u013F\x074\x02\x02\u013B\u013C\x07\x14\x02\x02\u013C" +
		"\u013E\x074\x02\x02\u013D\u013B\x03\x02\x02\x02\u013E\u0141\x03\x02\x02" +
		"\x02\u013F\u013D\x03\x02\x02\x02\u013F\u0140\x03\x02\x02\x02\u0140\u014B" +
		"\x03\x02\x02\x02\u0141\u013F\x03\x02\x02\x02\u0142\u0147\x05:\x1E\x02" +
		"\u0143\u0144\x07\x14\x02\x02\u0144\u0146\x05:\x1E\x02\u0145\u0143\x03" +
		"\x02\x02\x02\u0146\u0149\x03\x02\x02\x02\u0147\u0145\x03\x02\x02\x02\u0147" +
		"\u0148\x03\x02\x02\x02\u0148\u014B\x03\x02\x02\x02\u0149\u0147\x03\x02" +
		"\x02\x02\u014A\u013A\x03\x02\x02\x02\u014A\u0142\x03\x02\x02\x02\u014B" +
		"9\x03\x02\x02\x02\u014C\u014D\x07\t\x02\x02\u014D\u0152\x05<\x1F\x02\u014E" +
		"\u014F\x07\x14\x02\x02\u014F\u0151\x05<\x1F\x02\u0150\u014E\x03\x02\x02" +
		"\x02\u0151\u0154\x03\x02\x02\x02\u0152\u0150\x03\x02\x02\x02\u0152\u0153" +
		"\x03\x02\x02\x02\u0153\u0155\x03\x02\x02\x02\u0154\u0152\x03\x02\x02\x02" +
		"\u0155\u0156\x07\n\x02\x02\u0156;\x03\x02\x02\x02\u0157\u015A\x05Z.\x02" +
		"\u0158\u0159\x07\x12\x02\x02\u0159\u015B\x05b2\x02\u015A\u0158\x03\x02" +
		"\x02\x02\u015A\u015B\x03\x02\x02\x02\u015B=\x03\x02\x02\x02\u015C\u015D" +
		"\x07/\x02\x02\u015D\u015F\x05@!\x02\u015E\u0160\x05B\"\x02\u015F\u015E" +
		"\x03\x02\x02\x02\u015F\u0160\x03\x02\x02\x02\u0160\u0161\x03\x02\x02\x02" +
		"\u0161\u0162\x07\x13\x02\x02\u0162?\x03\x02\x02\x02\u0163\u0168\x075\x02" +
		"\x02\u0164\u0165\x07 \x02\x02\u0165\u0166\x07<\x02\x02\u0166\u0168\x07" +
		";\x02\x02\u0167\u0163\x03\x02\x02\x02\u0167\u0164\x03\x02\x02\x02\u0168" +
		"A\x03\x02\x02\x02\u0169\u016E\x074\x02\x02\u016A\u016B\x07\x14\x02\x02" +
		"\u016B\u016D\x074\x02\x02\u016C\u016A\x03\x02\x02\x02\u016D\u0170\x03" +
		"\x02\x02\x02\u016E\u016C\x03\x02\x02\x02\u016E\u016F\x03\x02\x02\x02\u016F" +
		"C\x03\x02\x02\x02\u0170\u016E\x03\x02\x02\x02\u0171\u0172\x07\x17\x02" +
		"\x02\u0172\u0173\x05F$\x02\u0173\u0174\x05L\'\x02\u0174\u0175\x07\v\x02" +
		"\x02\u0175\u0176\x05\x02\x02\x02\u0176\u0177\x07\f\x02\x02\u0177E\x03" +
		"\x02\x02\x02\u0178\u017C\t\x04\x02\x02\u0179\u017B\x074\x02\x02\u017A" +
		"\u0179\x03\x02\x02\x02\u017B\u017E\x03\x02\x02\x02\u017C\u017A\x03\x02" +
		"\x02\x02\u017C\u017D\x03\x02\x02\x02\u017D\u0182\x03\x02\x02\x02\u017E" +
		"\u017C\x03\x02\x02\x02\u017F\u0181\x05T+\x02\u0180\u017F\x03\x02\x02\x02" +
		"\u0181\u0184\x03\x02\x02\x02\u0182\u0180\x03\x02\x02\x02\u0182\u0183\x03" +
		"\x02\x02\x02\u0183G\x03\x02\x02\x02\u0184\u0182\x03\x02\x02\x02\u0185" +
		"\u0186\x05L\'\x02\u0186\u0187\x05J&\x02\u0187I\x03\x02\x02\x02\u0188\u018F" +
		"\x07\v\x02\x02\u0189\u018A\x05`1\x02\u018A\u018B\x07\x13\x02\x02\u018B" +
		"\u018E\x03\x02\x02\x02\u018C\u018E\x05\x04\x03\x02\u018D\u0189\x03\x02" +
		"\x02\x02\u018D\u018C\x03\x02\x02\x02\u018E\u0191\x03\x02\x02\x02\u018F" +
		"\u018D\x03\x02\x02\x02\u018F\u0190\x03\x02\x02\x02\u0190\u0193\x03\x02" +
		"\x02\x02\u0191\u018F\x03\x02\x02\x02\u0192\u0194\x05`1\x02\u0193\u0192" +
		"\x03\x02\x02\x02\u0193\u0194\x03\x02\x02\x02\u0194\u0195\x03\x02\x02\x02" +
		"\u0195\u0196\x07\f\x02\x02\u0196K\x03\x02\x02\x02\u0197\u019C\x05N(\x02" +
		"\u0198\u0199\x07\x14\x02\x02\u0199\u019B\x05N(\x02\u019A\u0198\x03\x02" +
		"\x02\x02\u019B\u019E\x03\x02\x02\x02\u019C\u019A\x03\x02\x02\x02\u019C" +
		"\u019D\x03\x02\x02\x02\u019DM\x03\x02\x02\x02\u019E\u019C\x03\x02\x02" +
		"\x02\u019F\u01A1\x05R*\x02\u01A0\u019F\x03\x02\x02\x02\u01A1\u01A2\x03" +
		"\x02\x02\x02\u01A2\u01A0\x03\x02\x02\x02\u01A2\u01A3\x03\x02\x02\x02\u01A3" +
		"\u01A9\x03\x02\x02\x02\u01A4\u01A5\x05P)\x02\u01A5\u01A6\x05R*\x02\u01A6" +
		"\u01A8\x03\x02\x02\x02\u01A7\u01A4\x03\x02\x02\x02\u01A8\u01AB\x03\x02" +
		"\x02\x02\u01A9\u01A7\x03\x02\x02\x02\u01A9\u01AA\x03\x02\x02\x02\u01AA" +
		"\u01AF\x03\x02\x02\x02\u01AB\u01A9\x03\x02\x02\x02\u01AC\u01AE\x05V,\x02" +
		"\u01AD\u01AC\x03\x02\x02\x02\u01AE\u01B1\x03\x02\x02\x02\u01AF\u01AD\x03" +
		"\x02\x02\x02\u01AF\u01B0\x03\x02\x02\x02\u01B0\u01B3\x03\x02\x02\x02\u01B1" +
		"\u01AF\x03\x02\x02\x02\u01B2\u01B4\x05T+\x02\u01B3\u01B2\x03\x02\x02\x02" +
		"\u01B3\u01B4\x03\x02\x02\x02\u01B4O\x03\x02\x02\x02\u01B5\u01B6\t\x05" +
		"\x02\x02\u01B6Q\x03\x02\x02\x02\u01B7\u01BF\x05Z.\x02\u01B8\u01B9\x07" +
		"\x19\x02\x02\u01B9\u01BF\x05Z.\x02\u01BA\u01BB\x07\x15\x02\x02\u01BB\u01BF" +
		"\x05Z.\x02\u01BC\u01BF\x07\x18\x02\x02\u01BD\u01BF\x07\x1C\x02\x02\u01BE" +
		"\u01B7\x03\x02\x02\x02\u01BE\u01B8\x03\x02\x02\x02\u01BE\u01BA\x03\x02" +
		"\x02\x02\u01BE\u01BC\x03\x02\x02\x02\u01BE\u01BD\x03\x02\x02\x02\u01BF" +
		"S\x03\x02\x02\x02\u01C0\u01C1\t\x06\x02\x02\u01C1\u01C5\x074\x02\x02\u01C2" +
		"\u01C3\t\x06\x02\x02\u01C3\u01C5\x05h5\x02\u01C4\u01C0\x03\x02\x02\x02" +
		"\u01C4\u01C2\x03\x02\x02\x02\u01C5U\x03\x02\x02\x02\u01C6\u01C7\x07\r" +
		"\x02\x02\u01C7\u01CB\x074\x02\x02\u01C8\u01C9\x05X-\x02\u01C9\u01CA\t" +
		"\x07\x02\x02\u01CA\u01CC\x03\x02\x02\x02\u01CB\u01C8\x03\x02\x02\x02\u01CB" +
		"\u01CC\x03\x02\x02\x02\u01CC\u01CD\x03\x02\x02\x02\u01CD\u01CE\x07\x0E" +
		"\x02\x02\u01CEW\x03\x02\x02\x02\u01CF\u01D0\t\b\x02\x02\u01D0Y\x03\x02" +
		"\x02\x02\u01D1\u01D5\x074\x02\x02\u01D2\u01D4\x05\\/\x02\u01D3\u01D2\x03" +
		"\x02\x02\x02\u01D4\u01D7\x03\x02\x02\x02\u01D5\u01D3\x03\x02\x02\x02\u01D5" +
		"\u01D6\x03\x02\x02\x02\u01D6\u01E2\x03\x02\x02\x02\u01D7\u01D5\x03\x02" +
		"\x02\x02\u01D8\u01D9\x07\b\x02\x02\u01D9\u01DA\x05^0\x02\u01DA\u01DE\x07" +
		"\f\x02\x02\u01DB\u01DD\x05\\/\x02\u01DC\u01DB\x03\x02\x02\x02\u01DD\u01E0" +
		"\x03\x02\x02\x02\u01DE\u01DC\x03\x02\x02\x02\u01DE\u01DF\x03\x02\x02\x02" +
		"\u01DF\u01E2\x03\x02\x02\x02\u01E0\u01DE\x03\x02\x02\x02\u01E1\u01D1\x03" +
		"\x02\x02\x02\u01E1\u01D8\x03\x02\x02\x02\u01E2[\x03\x02\x02\x02\u01E3" +
		"\u01E4\x07>\x02\x02\u01E4\u01E5\x05^0\x02\u01E5\u01E6\x07\f\x02\x02\u01E6" +
		"\u01E9\x03\x02\x02\x02\u01E7\u01E9\x07?\x02\x02\u01E8\u01E3\x03\x02\x02" +
		"\x02\u01E8\u01E7\x03\x02\x02\x02\u01E9]\x03\x02\x02\x02\u01EA\u01EB\x07" +
		"\x16\x02\x02\u01EB\u01EC\t\t\x02\x02\u01EC_\x03\x02\x02\x02\u01ED\u01EE" +
		"\x05Z.\x02\u01EE\u01EF\x07\x12\x02\x02\u01EF\u01F0\x05b2\x02\u01F0a\x03" +
		"\x02\x02\x02\u01F1\u01F6\x05\x1A\x0E\x02\u01F2\u01F3\x07\x14\x02\x02\u01F3" +
		"\u01F5\x05\x1A\x0E\x02\u01F4\u01F2\x03\x02\x02\x02\u01F5\u01F8\x03\x02" +
		"\x02\x02\u01F6\u01F4\x03\x02\x02\x02\u01F6\u01F7\x03\x02\x02\x02\u01F7" +
		"c\x03\x02\x02\x02\u01F8\u01F6\x03\x02\x02\x02\u01F9\u01FA\x07 \x02\x02" +
		"\u01FA\u01FB\x07<\x02\x02\u01FB\u01FC\x07;\x02\x02\u01FCe\x03\x02\x02" +
		"\x02\u01FD\u01FF\x076\x02\x02\u01FE\u0200\x07\x05\x02\x02\u01FF\u01FE" +
		"\x03\x02\x02\x02\u01FF\u0200\x03\x02\x02\x02\u0200g\x03\x02\x02\x02\u0201" +
		"\u0202\x074\x02\x02\u0202\u0204\x07\t\x02\x02\u0203\u0205\x05b2\x02\u0204" +
		"\u0203\x03\x02\x02\x02\u0204\u0205\x03\x02\x02\x02\u0205\u0206\x03\x02" +
		"\x02\x02\u0206\u0207\x07\n\x02\x02\u0207i\x03\x02\x02\x02;m{\x82\x86\x8A" +
		"\x93\x99\x9C\xA5\xA9\xAB\xAE\xB0\xB6\xBB\xC2\xCF\xD4\xDA\xDD\xEC\xF4\xF8" +
		"\u0105\u0108\u010D\u0113\u0119\u0133\u013F\u0147\u014A\u0152\u015A\u015F" +
		"\u0167\u016E\u017C\u0182\u018D\u018F\u0193\u019C\u01A2\u01A9\u01AF\u01B3" +
		"\u01BE\u01C4\u01CB\u01D5\u01DE\u01E1\u01E8\u01F6\u01FF\u0204";
	public static __ATN: ATN;
	public static get _ATN(): ATN {
		if (!ScssParser.__ATN) {
			ScssParser.__ATN = new ATNDeserializer().deserialize(Utils.toCharArray(ScssParser._serializedATN));
		}

		return ScssParser.__ATN;
	}

}

export class StylesheetContext extends ParserRuleContext {
	public statement(): StatementContext[];
	public statement(i: number): StatementContext;
	public statement(i?: number): StatementContext | StatementContext[] {
		if (i === undefined) {
			return this.getRuleContexts(StatementContext);
		} else {
			return this.getRuleContext(i, StatementContext);
		}
	}
	constructor(parent: ParserRuleContext | undefined, invokingState: number) {
		super(parent, invokingState);
	}
	// @Override
	public get ruleIndex(): number { return ScssParser.RULE_stylesheet; }
	// @Override
	public enterRule(listener: ScssParserListener): void {
		if (listener.enterStylesheet) {
			listener.enterStylesheet(this);
		}
	}
	// @Override
	public exitRule(listener: ScssParserListener): void {
		if (listener.exitStylesheet) {
			listener.exitStylesheet(this);
		}
	}
	// @Override
	public accept<Result>(visitor: ScssParserVisitor<Result>): Result {
		if (visitor.visitStylesheet) {
			return visitor.visitStylesheet(this);
		} else {
			return visitor.visitChildren(this);
		}
	}
}


export class StatementContext extends ParserRuleContext {
	public importDeclaration(): ImportDeclarationContext | undefined {
		return this.tryGetRuleContext(0, ImportDeclarationContext);
	}
	public nested(): NestedContext | undefined {
		return this.tryGetRuleContext(0, NestedContext);
	}
	public ruleset(): RulesetContext | undefined {
		return this.tryGetRuleContext(0, RulesetContext);
	}
	public mixinDeclaration(): MixinDeclarationContext | undefined {
		return this.tryGetRuleContext(0, MixinDeclarationContext);
	}
	public functionDeclaration(): FunctionDeclarationContext | undefined {
		return this.tryGetRuleContext(0, FunctionDeclarationContext);
	}
	public variableDeclaration(): VariableDeclarationContext | undefined {
		return this.tryGetRuleContext(0, VariableDeclarationContext);
	}
	public includeDeclaration(): IncludeDeclarationContext | undefined {
		return this.tryGetRuleContext(0, IncludeDeclarationContext);
	}
	public ifDeclaration(): IfDeclarationContext | undefined {
		return this.tryGetRuleContext(0, IfDeclarationContext);
	}
	public forDeclaration(): ForDeclarationContext | undefined {
		return this.tryGetRuleContext(0, ForDeclarationContext);
	}
	public whileDeclaration(): WhileDeclarationContext | undefined {
		return this.tryGetRuleContext(0, WhileDeclarationContext);
	}
	public eachDeclaration(): EachDeclarationContext | undefined {
		return this.tryGetRuleContext(0, EachDeclarationContext);
	}
	constructor(parent: ParserRuleContext | undefined, invokingState: number) {
		super(parent, invokingState);
	}
	// @Override
	public get ruleIndex(): number { return ScssParser.RULE_statement; }
	// @Override
	public enterRule(listener: ScssParserListener): void {
		if (listener.enterStatement) {
			listener.enterStatement(this);
		}
	}
	// @Override
	public exitRule(listener: ScssParserListener): void {
		if (listener.exitStatement) {
			listener.exitStatement(this);
		}
	}
	// @Override
	public accept<Result>(visitor: ScssParserVisitor<Result>): Result {
		if (visitor.visitStatement) {
			return visitor.visitStatement(this);
		} else {
			return visitor.visitChildren(this);
		}
	}
}


export class ParamsContext extends ParserRuleContext {
	public param(): ParamContext[];
	public param(i: number): ParamContext;
	public param(i?: number): ParamContext | ParamContext[] {
		if (i === undefined) {
			return this.getRuleContexts(ParamContext);
		} else {
			return this.getRuleContext(i, ParamContext);
		}
	}
	public COMMA(): TerminalNode[];
	public COMMA(i: number): TerminalNode;
	public COMMA(i?: number): TerminalNode | TerminalNode[] {
		if (i === undefined) {
			return this.getTokens(ScssParser.COMMA);
		} else {
			return this.getToken(ScssParser.COMMA, i);
		}
	}
	public Ellipsis(): TerminalNode | undefined { return this.tryGetToken(ScssParser.Ellipsis, 0); }
	constructor(parent: ParserRuleContext | undefined, invokingState: number) {
		super(parent, invokingState);
	}
	// @Override
	public get ruleIndex(): number { return ScssParser.RULE_params; }
	// @Override
	public enterRule(listener: ScssParserListener): void {
		if (listener.enterParams) {
			listener.enterParams(this);
		}
	}
	// @Override
	public exitRule(listener: ScssParserListener): void {
		if (listener.exitParams) {
			listener.exitParams(this);
		}
	}
	// @Override
	public accept<Result>(visitor: ScssParserVisitor<Result>): Result {
		if (visitor.visitParams) {
			return visitor.visitParams(this);
		} else {
			return visitor.visitChildren(this);
		}
	}
}


export class ParamContext extends ParserRuleContext {
	public variableName(): VariableNameContext {
		return this.getRuleContext(0, VariableNameContext);
	}
	public paramOptionalValue(): ParamOptionalValueContext | undefined {
		return this.tryGetRuleContext(0, ParamOptionalValueContext);
	}
	constructor(parent: ParserRuleContext | undefined, invokingState: number) {
		super(parent, invokingState);
	}
	// @Override
	public get ruleIndex(): number { return ScssParser.RULE_param; }
	// @Override
	public enterRule(listener: ScssParserListener): void {
		if (listener.enterParam) {
			listener.enterParam(this);
		}
	}
	// @Override
	public exitRule(listener: ScssParserListener): void {
		if (listener.exitParam) {
			listener.exitParam(this);
		}
	}
	// @Override
	public accept<Result>(visitor: ScssParserVisitor<Result>): Result {
		if (visitor.visitParam) {
			return visitor.visitParam(this);
		} else {
			return visitor.visitChildren(this);
		}
	}
}


export class VariableNameContext extends ParserRuleContext {
	public DOLLAR(): TerminalNode { return this.getToken(ScssParser.DOLLAR, 0); }
	public Identifier(): TerminalNode { return this.getToken(ScssParser.Identifier, 0); }
	constructor(parent: ParserRuleContext | undefined, invokingState: number) {
		super(parent, invokingState);
	}
	// @Override
	public get ruleIndex(): number { return ScssParser.RULE_variableName; }
	// @Override
	public enterRule(listener: ScssParserListener): void {
		if (listener.enterVariableName) {
			listener.enterVariableName(this);
		}
	}
	// @Override
	public exitRule(listener: ScssParserListener): void {
		if (listener.exitVariableName) {
			listener.exitVariableName(this);
		}
	}
	// @Override
	public accept<Result>(visitor: ScssParserVisitor<Result>): Result {
		if (visitor.visitVariableName) {
			return visitor.visitVariableName(this);
		} else {
			return visitor.visitChildren(this);
		}
	}
}


export class ParamOptionalValueContext extends ParserRuleContext {
	public COLON(): TerminalNode { return this.getToken(ScssParser.COLON, 0); }
	public expression(): ExpressionContext[];
	public expression(i: number): ExpressionContext;
	public expression(i?: number): ExpressionContext | ExpressionContext[] {
		if (i === undefined) {
			return this.getRuleContexts(ExpressionContext);
		} else {
			return this.getRuleContext(i, ExpressionContext);
		}
	}
	constructor(parent: ParserRuleContext | undefined, invokingState: number) {
		super(parent, invokingState);
	}
	// @Override
	public get ruleIndex(): number { return ScssParser.RULE_paramOptionalValue; }
	// @Override
	public enterRule(listener: ScssParserListener): void {
		if (listener.enterParamOptionalValue) {
			listener.enterParamOptionalValue(this);
		}
	}
	// @Override
	public exitRule(listener: ScssParserListener): void {
		if (listener.exitParamOptionalValue) {
			listener.exitParamOptionalValue(this);
		}
	}
	// @Override
	public accept<Result>(visitor: ScssParserVisitor<Result>): Result {
		if (visitor.visitParamOptionalValue) {
			return visitor.visitParamOptionalValue(this);
		} else {
			return visitor.visitChildren(this);
		}
	}
}


export class MixinDeclarationContext extends ParserRuleContext {
	public MIXIN(): TerminalNode { return this.getToken(ScssParser.MIXIN, 0); }
	public Identifier(): TerminalNode { return this.getToken(ScssParser.Identifier, 0); }
	public block(): BlockContext {
		return this.getRuleContext(0, BlockContext);
	}
	public LPAREN(): TerminalNode | undefined { return this.tryGetToken(ScssParser.LPAREN, 0); }
	public RPAREN(): TerminalNode | undefined { return this.tryGetToken(ScssParser.RPAREN, 0); }
	public params(): ParamsContext | undefined {
		return this.tryGetRuleContext(0, ParamsContext);
	}
	constructor(parent: ParserRuleContext | undefined, invokingState: number) {
		super(parent, invokingState);
	}
	// @Override
	public get ruleIndex(): number { return ScssParser.RULE_mixinDeclaration; }
	// @Override
	public enterRule(listener: ScssParserListener): void {
		if (listener.enterMixinDeclaration) {
			listener.enterMixinDeclaration(this);
		}
	}
	// @Override
	public exitRule(listener: ScssParserListener): void {
		if (listener.exitMixinDeclaration) {
			listener.exitMixinDeclaration(this);
		}
	}
	// @Override
	public accept<Result>(visitor: ScssParserVisitor<Result>): Result {
		if (visitor.visitMixinDeclaration) {
			return visitor.visitMixinDeclaration(this);
		} else {
			return visitor.visitChildren(this);
		}
	}
}


export class IncludeDeclarationContext extends ParserRuleContext {
	public INCLUDE(): TerminalNode { return this.getToken(ScssParser.INCLUDE, 0); }
	public Identifier(): TerminalNode { return this.getToken(ScssParser.Identifier, 0); }
	public SEMI(): TerminalNode | undefined { return this.tryGetToken(ScssParser.SEMI, 0); }
	public LPAREN(): TerminalNode | undefined { return this.tryGetToken(ScssParser.LPAREN, 0); }
	public RPAREN(): TerminalNode | undefined { return this.tryGetToken(ScssParser.RPAREN, 0); }
	public block(): BlockContext | undefined {
		return this.tryGetRuleContext(0, BlockContext);
	}
	public values(): ValuesContext | undefined {
		return this.tryGetRuleContext(0, ValuesContext);
	}
	constructor(parent: ParserRuleContext | undefined, invokingState: number) {
		super(parent, invokingState);
	}
	// @Override
	public get ruleIndex(): number { return ScssParser.RULE_includeDeclaration; }
	// @Override
	public enterRule(listener: ScssParserListener): void {
		if (listener.enterIncludeDeclaration) {
			listener.enterIncludeDeclaration(this);
		}
	}
	// @Override
	public exitRule(listener: ScssParserListener): void {
		if (listener.exitIncludeDeclaration) {
			listener.exitIncludeDeclaration(this);
		}
	}
	// @Override
	public accept<Result>(visitor: ScssParserVisitor<Result>): Result {
		if (visitor.visitIncludeDeclaration) {
			return visitor.visitIncludeDeclaration(this);
		} else {
			return visitor.visitChildren(this);
		}
	}
}


export class FunctionDeclarationContext extends ParserRuleContext {
	public FUNCTION(): TerminalNode { return this.getToken(ScssParser.FUNCTION, 0); }
	public Identifier(): TerminalNode { return this.getToken(ScssParser.Identifier, 0); }
	public LPAREN(): TerminalNode { return this.getToken(ScssParser.LPAREN, 0); }
	public RPAREN(): TerminalNode { return this.getToken(ScssParser.RPAREN, 0); }
	public BlockStart(): TerminalNode { return this.getToken(ScssParser.BlockStart, 0); }
	public BlockEnd(): TerminalNode { return this.getToken(ScssParser.BlockEnd, 0); }
	public params(): ParamsContext | undefined {
		return this.tryGetRuleContext(0, ParamsContext);
	}
	public functionBody(): FunctionBodyContext | undefined {
		return this.tryGetRuleContext(0, FunctionBodyContext);
	}
	constructor(parent: ParserRuleContext | undefined, invokingState: number) {
		super(parent, invokingState);
	}
	// @Override
	public get ruleIndex(): number { return ScssParser.RULE_functionDeclaration; }
	// @Override
	public enterRule(listener: ScssParserListener): void {
		if (listener.enterFunctionDeclaration) {
			listener.enterFunctionDeclaration(this);
		}
	}
	// @Override
	public exitRule(listener: ScssParserListener): void {
		if (listener.exitFunctionDeclaration) {
			listener.exitFunctionDeclaration(this);
		}
	}
	// @Override
	public accept<Result>(visitor: ScssParserVisitor<Result>): Result {
		if (visitor.visitFunctionDeclaration) {
			return visitor.visitFunctionDeclaration(this);
		} else {
			return visitor.visitChildren(this);
		}
	}
}


export class FunctionBodyContext extends ParserRuleContext {
	public functionReturn(): FunctionReturnContext {
		return this.getRuleContext(0, FunctionReturnContext);
	}
	public functionStatement(): FunctionStatementContext[];
	public functionStatement(i: number): FunctionStatementContext;
	public functionStatement(i?: number): FunctionStatementContext | FunctionStatementContext[] {
		if (i === undefined) {
			return this.getRuleContexts(FunctionStatementContext);
		} else {
			return this.getRuleContext(i, FunctionStatementContext);
		}
	}
	constructor(parent: ParserRuleContext | undefined, invokingState: number) {
		super(parent, invokingState);
	}
	// @Override
	public get ruleIndex(): number { return ScssParser.RULE_functionBody; }
	// @Override
	public enterRule(listener: ScssParserListener): void {
		if (listener.enterFunctionBody) {
			listener.enterFunctionBody(this);
		}
	}
	// @Override
	public exitRule(listener: ScssParserListener): void {
		if (listener.exitFunctionBody) {
			listener.exitFunctionBody(this);
		}
	}
	// @Override
	public accept<Result>(visitor: ScssParserVisitor<Result>): Result {
		if (visitor.visitFunctionBody) {
			return visitor.visitFunctionBody(this);
		} else {
			return visitor.visitChildren(this);
		}
	}
}


export class FunctionReturnContext extends ParserRuleContext {
	public RETURN(): TerminalNode { return this.getToken(ScssParser.RETURN, 0); }
	public commandStatement(): CommandStatementContext {
		return this.getRuleContext(0, CommandStatementContext);
	}
	public SEMI(): TerminalNode { return this.getToken(ScssParser.SEMI, 0); }
	constructor(parent: ParserRuleContext | undefined, invokingState: number) {
		super(parent, invokingState);
	}
	// @Override
	public get ruleIndex(): number { return ScssParser.RULE_functionReturn; }
	// @Override
	public enterRule(listener: ScssParserListener): void {
		if (listener.enterFunctionReturn) {
			listener.enterFunctionReturn(this);
		}
	}
	// @Override
	public exitRule(listener: ScssParserListener): void {
		if (listener.exitFunctionReturn) {
			listener.exitFunctionReturn(this);
		}
	}
	// @Override
	public accept<Result>(visitor: ScssParserVisitor<Result>): Result {
		if (visitor.visitFunctionReturn) {
			return visitor.visitFunctionReturn(this);
		} else {
			return visitor.visitChildren(this);
		}
	}
}


export class FunctionStatementContext extends ParserRuleContext {
	public commandStatement(): CommandStatementContext | undefined {
		return this.tryGetRuleContext(0, CommandStatementContext);
	}
	public SEMI(): TerminalNode | undefined { return this.tryGetToken(ScssParser.SEMI, 0); }
	public statement(): StatementContext | undefined {
		return this.tryGetRuleContext(0, StatementContext);
	}
	constructor(parent: ParserRuleContext | undefined, invokingState: number) {
		super(parent, invokingState);
	}
	// @Override
	public get ruleIndex(): number { return ScssParser.RULE_functionStatement; }
	// @Override
	public enterRule(listener: ScssParserListener): void {
		if (listener.enterFunctionStatement) {
			listener.enterFunctionStatement(this);
		}
	}
	// @Override
	public exitRule(listener: ScssParserListener): void {
		if (listener.exitFunctionStatement) {
			listener.exitFunctionStatement(this);
		}
	}
	// @Override
	public accept<Result>(visitor: ScssParserVisitor<Result>): Result {
		if (visitor.visitFunctionStatement) {
			return visitor.visitFunctionStatement(this);
		} else {
			return visitor.visitChildren(this);
		}
	}
}


export class CommandStatementContext extends ParserRuleContext {
	public LPAREN(): TerminalNode | undefined { return this.tryGetToken(ScssParser.LPAREN, 0); }
	public commandStatement(): CommandStatementContext | undefined {
		return this.tryGetRuleContext(0, CommandStatementContext);
	}
	public RPAREN(): TerminalNode | undefined { return this.tryGetToken(ScssParser.RPAREN, 0); }
	public mathStatement(): MathStatementContext | undefined {
		return this.tryGetRuleContext(0, MathStatementContext);
	}
	public expression(): ExpressionContext[];
	public expression(i: number): ExpressionContext;
	public expression(i?: number): ExpressionContext | ExpressionContext[] {
		if (i === undefined) {
			return this.getRuleContexts(ExpressionContext);
		} else {
			return this.getRuleContext(i, ExpressionContext);
		}
	}
	constructor(parent: ParserRuleContext | undefined, invokingState: number) {
		super(parent, invokingState);
	}
	// @Override
	public get ruleIndex(): number { return ScssParser.RULE_commandStatement; }
	// @Override
	public enterRule(listener: ScssParserListener): void {
		if (listener.enterCommandStatement) {
			listener.enterCommandStatement(this);
		}
	}
	// @Override
	public exitRule(listener: ScssParserListener): void {
		if (listener.exitCommandStatement) {
			listener.exitCommandStatement(this);
		}
	}
	// @Override
	public accept<Result>(visitor: ScssParserVisitor<Result>): Result {
		if (visitor.visitCommandStatement) {
			return visitor.visitCommandStatement(this);
		} else {
			return visitor.visitChildren(this);
		}
	}
}


export class MathCharacterContext extends ParserRuleContext {
	public TIMES(): TerminalNode | undefined { return this.tryGetToken(ScssParser.TIMES, 0); }
	public PLUS(): TerminalNode | undefined { return this.tryGetToken(ScssParser.PLUS, 0); }
	public DIV(): TerminalNode | undefined { return this.tryGetToken(ScssParser.DIV, 0); }
	public MINUS(): TerminalNode | undefined { return this.tryGetToken(ScssParser.MINUS, 0); }
	public PERC(): TerminalNode | undefined { return this.tryGetToken(ScssParser.PERC, 0); }
	constructor(parent: ParserRuleContext | undefined, invokingState: number) {
		super(parent, invokingState);
	}
	// @Override
	public get ruleIndex(): number { return ScssParser.RULE_mathCharacter; }
	// @Override
	public enterRule(listener: ScssParserListener): void {
		if (listener.enterMathCharacter) {
			listener.enterMathCharacter(this);
		}
	}
	// @Override
	public exitRule(listener: ScssParserListener): void {
		if (listener.exitMathCharacter) {
			listener.exitMathCharacter(this);
		}
	}
	// @Override
	public accept<Result>(visitor: ScssParserVisitor<Result>): Result {
		if (visitor.visitMathCharacter) {
			return visitor.visitMathCharacter(this);
		} else {
			return visitor.visitChildren(this);
		}
	}
}


export class MathStatementContext extends ParserRuleContext {
	public mathCharacter(): MathCharacterContext {
		return this.getRuleContext(0, MathCharacterContext);
	}
	public commandStatement(): CommandStatementContext {
		return this.getRuleContext(0, CommandStatementContext);
	}
	constructor(parent: ParserRuleContext | undefined, invokingState: number) {
		super(parent, invokingState);
	}
	// @Override
	public get ruleIndex(): number { return ScssParser.RULE_mathStatement; }
	// @Override
	public enterRule(listener: ScssParserListener): void {
		if (listener.enterMathStatement) {
			listener.enterMathStatement(this);
		}
	}
	// @Override
	public exitRule(listener: ScssParserListener): void {
		if (listener.exitMathStatement) {
			listener.exitMathStatement(this);
		}
	}
	// @Override
	public accept<Result>(visitor: ScssParserVisitor<Result>): Result {
		if (visitor.visitMathStatement) {
			return visitor.visitMathStatement(this);
		} else {
			return visitor.visitChildren(this);
		}
	}
}


export class ExpressionContext extends ParserRuleContext {
	public measurement(): MeasurementContext | undefined {
		return this.tryGetRuleContext(0, MeasurementContext);
	}
	public identifier(): IdentifierContext | undefined {
		return this.tryGetRuleContext(0, IdentifierContext);
	}
	public Color(): TerminalNode | undefined { return this.tryGetToken(ScssParser.Color, 0); }
	public StringLiteral(): TerminalNode | undefined { return this.tryGetToken(ScssParser.StringLiteral, 0); }
	public NULL(): TerminalNode | undefined { return this.tryGetToken(ScssParser.NULL, 0); }
	public url(): UrlContext | undefined {
		return this.tryGetRuleContext(0, UrlContext);
	}
	public variableName(): VariableNameContext | undefined {
		return this.tryGetRuleContext(0, VariableNameContext);
	}
	public functionCall(): FunctionCallContext | undefined {
		return this.tryGetRuleContext(0, FunctionCallContext);
	}
	constructor(parent: ParserRuleContext | undefined, invokingState: number) {
		super(parent, invokingState);
	}
	// @Override
	public get ruleIndex(): number { return ScssParser.RULE_expression; }
	// @Override
	public enterRule(listener: ScssParserListener): void {
		if (listener.enterExpression) {
			listener.enterExpression(this);
		}
	}
	// @Override
	public exitRule(listener: ScssParserListener): void {
		if (listener.exitExpression) {
			listener.exitExpression(this);
		}
	}
	// @Override
	public accept<Result>(visitor: ScssParserVisitor<Result>): Result {
		if (visitor.visitExpression) {
			return visitor.visitExpression(this);
		} else {
			return visitor.visitChildren(this);
		}
	}
}


export class IfDeclarationContext extends ParserRuleContext {
	public AT_IF(): TerminalNode { return this.getToken(ScssParser.AT_IF, 0); }
	public conditions(): ConditionsContext {
		return this.getRuleContext(0, ConditionsContext);
	}
	public block(): BlockContext {
		return this.getRuleContext(0, BlockContext);
	}
	public elseIfStatement(): ElseIfStatementContext[];
	public elseIfStatement(i: number): ElseIfStatementContext;
	public elseIfStatement(i?: number): ElseIfStatementContext | ElseIfStatementContext[] {
		if (i === undefined) {
			return this.getRuleContexts(ElseIfStatementContext);
		} else {
			return this.getRuleContext(i, ElseIfStatementContext);
		}
	}
	public elseStatement(): ElseStatementContext | undefined {
		return this.tryGetRuleContext(0, ElseStatementContext);
	}
	constructor(parent: ParserRuleContext | undefined, invokingState: number) {
		super(parent, invokingState);
	}
	// @Override
	public get ruleIndex(): number { return ScssParser.RULE_ifDeclaration; }
	// @Override
	public enterRule(listener: ScssParserListener): void {
		if (listener.enterIfDeclaration) {
			listener.enterIfDeclaration(this);
		}
	}
	// @Override
	public exitRule(listener: ScssParserListener): void {
		if (listener.exitIfDeclaration) {
			listener.exitIfDeclaration(this);
		}
	}
	// @Override
	public accept<Result>(visitor: ScssParserVisitor<Result>): Result {
		if (visitor.visitIfDeclaration) {
			return visitor.visitIfDeclaration(this);
		} else {
			return visitor.visitChildren(this);
		}
	}
}


export class ElseIfStatementContext extends ParserRuleContext {
	public AT_ELSE(): TerminalNode { return this.getToken(ScssParser.AT_ELSE, 0); }
	public IF(): TerminalNode { return this.getToken(ScssParser.IF, 0); }
	public conditions(): ConditionsContext {
		return this.getRuleContext(0, ConditionsContext);
	}
	public block(): BlockContext {
		return this.getRuleContext(0, BlockContext);
	}
	constructor(parent: ParserRuleContext | undefined, invokingState: number) {
		super(parent, invokingState);
	}
	// @Override
	public get ruleIndex(): number { return ScssParser.RULE_elseIfStatement; }
	// @Override
	public enterRule(listener: ScssParserListener): void {
		if (listener.enterElseIfStatement) {
			listener.enterElseIfStatement(this);
		}
	}
	// @Override
	public exitRule(listener: ScssParserListener): void {
		if (listener.exitElseIfStatement) {
			listener.exitElseIfStatement(this);
		}
	}
	// @Override
	public accept<Result>(visitor: ScssParserVisitor<Result>): Result {
		if (visitor.visitElseIfStatement) {
			return visitor.visitElseIfStatement(this);
		} else {
			return visitor.visitChildren(this);
		}
	}
}


export class ElseStatementContext extends ParserRuleContext {
	public AT_ELSE(): TerminalNode { return this.getToken(ScssParser.AT_ELSE, 0); }
	public block(): BlockContext {
		return this.getRuleContext(0, BlockContext);
	}
	constructor(parent: ParserRuleContext | undefined, invokingState: number) {
		super(parent, invokingState);
	}
	// @Override
	public get ruleIndex(): number { return ScssParser.RULE_elseStatement; }
	// @Override
	public enterRule(listener: ScssParserListener): void {
		if (listener.enterElseStatement) {
			listener.enterElseStatement(this);
		}
	}
	// @Override
	public exitRule(listener: ScssParserListener): void {
		if (listener.exitElseStatement) {
			listener.exitElseStatement(this);
		}
	}
	// @Override
	public accept<Result>(visitor: ScssParserVisitor<Result>): Result {
		if (visitor.visitElseStatement) {
			return visitor.visitElseStatement(this);
		} else {
			return visitor.visitChildren(this);
		}
	}
}


export class ConditionsContext extends ParserRuleContext {
	public condition(): ConditionContext | undefined {
		return this.tryGetRuleContext(0, ConditionContext);
	}
	public COMBINE_COMPARE(): TerminalNode | undefined { return this.tryGetToken(ScssParser.COMBINE_COMPARE, 0); }
	public conditions(): ConditionsContext | undefined {
		return this.tryGetRuleContext(0, ConditionsContext);
	}
	public NULL(): TerminalNode | undefined { return this.tryGetToken(ScssParser.NULL, 0); }
	constructor(parent: ParserRuleContext | undefined, invokingState: number) {
		super(parent, invokingState);
	}
	// @Override
	public get ruleIndex(): number { return ScssParser.RULE_conditions; }
	// @Override
	public enterRule(listener: ScssParserListener): void {
		if (listener.enterConditions) {
			listener.enterConditions(this);
		}
	}
	// @Override
	public exitRule(listener: ScssParserListener): void {
		if (listener.exitConditions) {
			listener.exitConditions(this);
		}
	}
	// @Override
	public accept<Result>(visitor: ScssParserVisitor<Result>): Result {
		if (visitor.visitConditions) {
			return visitor.visitConditions(this);
		} else {
			return visitor.visitChildren(this);
		}
	}
}


export class ConditionContext extends ParserRuleContext {
	public commandStatement(): CommandStatementContext | undefined {
		return this.tryGetRuleContext(0, CommandStatementContext);
	}
	public conditions(): ConditionsContext | undefined {
		return this.tryGetRuleContext(0, ConditionsContext);
	}
	public EQEQ(): TerminalNode | undefined { return this.tryGetToken(ScssParser.EQEQ, 0); }
	public LT(): TerminalNode | undefined { return this.tryGetToken(ScssParser.LT, 0); }
	public GT(): TerminalNode | undefined { return this.tryGetToken(ScssParser.GT, 0); }
	public NOTEQ(): TerminalNode | undefined { return this.tryGetToken(ScssParser.NOTEQ, 0); }
	public LPAREN(): TerminalNode | undefined { return this.tryGetToken(ScssParser.LPAREN, 0); }
	public RPAREN(): TerminalNode | undefined { return this.tryGetToken(ScssParser.RPAREN, 0); }
	constructor(parent: ParserRuleContext | undefined, invokingState: number) {
		super(parent, invokingState);
	}
	// @Override
	public get ruleIndex(): number { return ScssParser.RULE_condition; }
	// @Override
	public enterRule(listener: ScssParserListener): void {
		if (listener.enterCondition) {
			listener.enterCondition(this);
		}
	}
	// @Override
	public exitRule(listener: ScssParserListener): void {
		if (listener.exitCondition) {
			listener.exitCondition(this);
		}
	}
	// @Override
	public accept<Result>(visitor: ScssParserVisitor<Result>): Result {
		if (visitor.visitCondition) {
			return visitor.visitCondition(this);
		} else {
			return visitor.visitChildren(this);
		}
	}
}


export class VariableDeclarationContext extends ParserRuleContext {
	public variableName(): VariableNameContext {
		return this.getRuleContext(0, VariableNameContext);
	}
	public COLON(): TerminalNode { return this.getToken(ScssParser.COLON, 0); }
	public values(): ValuesContext {
		return this.getRuleContext(0, ValuesContext);
	}
	public SEMI(): TerminalNode { return this.getToken(ScssParser.SEMI, 0); }
	public POUND_DEFAULT(): TerminalNode | undefined { return this.tryGetToken(ScssParser.POUND_DEFAULT, 0); }
	constructor(parent: ParserRuleContext | undefined, invokingState: number) {
		super(parent, invokingState);
	}
	// @Override
	public get ruleIndex(): number { return ScssParser.RULE_variableDeclaration; }
	// @Override
	public enterRule(listener: ScssParserListener): void {
		if (listener.enterVariableDeclaration) {
			listener.enterVariableDeclaration(this);
		}
	}
	// @Override
	public exitRule(listener: ScssParserListener): void {
		if (listener.exitVariableDeclaration) {
			listener.exitVariableDeclaration(this);
		}
	}
	// @Override
	public accept<Result>(visitor: ScssParserVisitor<Result>): Result {
		if (visitor.visitVariableDeclaration) {
			return visitor.visitVariableDeclaration(this);
		} else {
			return visitor.visitChildren(this);
		}
	}
}


export class ForDeclarationContext extends ParserRuleContext {
	public AT_FOR(): TerminalNode { return this.getToken(ScssParser.AT_FOR, 0); }
	public variableName(): VariableNameContext {
		return this.getRuleContext(0, VariableNameContext);
	}
	public FROM(): TerminalNode { return this.getToken(ScssParser.FROM, 0); }
	public fromNumber(): FromNumberContext {
		return this.getRuleContext(0, FromNumberContext);
	}
	public THROUGH(): TerminalNode { return this.getToken(ScssParser.THROUGH, 0); }
	public throughNumber(): ThroughNumberContext {
		return this.getRuleContext(0, ThroughNumberContext);
	}
	public block(): BlockContext {
		return this.getRuleContext(0, BlockContext);
	}
	constructor(parent: ParserRuleContext | undefined, invokingState: number) {
		super(parent, invokingState);
	}
	// @Override
	public get ruleIndex(): number { return ScssParser.RULE_forDeclaration; }
	// @Override
	public enterRule(listener: ScssParserListener): void {
		if (listener.enterForDeclaration) {
			listener.enterForDeclaration(this);
		}
	}
	// @Override
	public exitRule(listener: ScssParserListener): void {
		if (listener.exitForDeclaration) {
			listener.exitForDeclaration(this);
		}
	}
	// @Override
	public accept<Result>(visitor: ScssParserVisitor<Result>): Result {
		if (visitor.visitForDeclaration) {
			return visitor.visitForDeclaration(this);
		} else {
			return visitor.visitChildren(this);
		}
	}
}


export class FromNumberContext extends ParserRuleContext {
	public Number(): TerminalNode { return this.getToken(ScssParser.Number, 0); }
	constructor(parent: ParserRuleContext | undefined, invokingState: number) {
		super(parent, invokingState);
	}
	// @Override
	public get ruleIndex(): number { return ScssParser.RULE_fromNumber; }
	// @Override
	public enterRule(listener: ScssParserListener): void {
		if (listener.enterFromNumber) {
			listener.enterFromNumber(this);
		}
	}
	// @Override
	public exitRule(listener: ScssParserListener): void {
		if (listener.exitFromNumber) {
			listener.exitFromNumber(this);
		}
	}
	// @Override
	public accept<Result>(visitor: ScssParserVisitor<Result>): Result {
		if (visitor.visitFromNumber) {
			return visitor.visitFromNumber(this);
		} else {
			return visitor.visitChildren(this);
		}
	}
}


export class ThroughNumberContext extends ParserRuleContext {
	public Number(): TerminalNode { return this.getToken(ScssParser.Number, 0); }
	constructor(parent: ParserRuleContext | undefined, invokingState: number) {
		super(parent, invokingState);
	}
	// @Override
	public get ruleIndex(): number { return ScssParser.RULE_throughNumber; }
	// @Override
	public enterRule(listener: ScssParserListener): void {
		if (listener.enterThroughNumber) {
			listener.enterThroughNumber(this);
		}
	}
	// @Override
	public exitRule(listener: ScssParserListener): void {
		if (listener.exitThroughNumber) {
			listener.exitThroughNumber(this);
		}
	}
	// @Override
	public accept<Result>(visitor: ScssParserVisitor<Result>): Result {
		if (visitor.visitThroughNumber) {
			return visitor.visitThroughNumber(this);
		} else {
			return visitor.visitChildren(this);
		}
	}
}


export class WhileDeclarationContext extends ParserRuleContext {
	public AT_WHILE(): TerminalNode { return this.getToken(ScssParser.AT_WHILE, 0); }
	public conditions(): ConditionsContext {
		return this.getRuleContext(0, ConditionsContext);
	}
	public block(): BlockContext {
		return this.getRuleContext(0, BlockContext);
	}
	constructor(parent: ParserRuleContext | undefined, invokingState: number) {
		super(parent, invokingState);
	}
	// @Override
	public get ruleIndex(): number { return ScssParser.RULE_whileDeclaration; }
	// @Override
	public enterRule(listener: ScssParserListener): void {
		if (listener.enterWhileDeclaration) {
			listener.enterWhileDeclaration(this);
		}
	}
	// @Override
	public exitRule(listener: ScssParserListener): void {
		if (listener.exitWhileDeclaration) {
			listener.exitWhileDeclaration(this);
		}
	}
	// @Override
	public accept<Result>(visitor: ScssParserVisitor<Result>): Result {
		if (visitor.visitWhileDeclaration) {
			return visitor.visitWhileDeclaration(this);
		} else {
			return visitor.visitChildren(this);
		}
	}
}


export class EachDeclarationContext extends ParserRuleContext {
	public AT_EACH(): TerminalNode { return this.getToken(ScssParser.AT_EACH, 0); }
	public variableName(): VariableNameContext[];
	public variableName(i: number): VariableNameContext;
	public variableName(i?: number): VariableNameContext | VariableNameContext[] {
		if (i === undefined) {
			return this.getRuleContexts(VariableNameContext);
		} else {
			return this.getRuleContext(i, VariableNameContext);
		}
	}
	public IN(): TerminalNode { return this.getToken(ScssParser.IN, 0); }
	public eachValueList(): EachValueListContext {
		return this.getRuleContext(0, EachValueListContext);
	}
	public block(): BlockContext {
		return this.getRuleContext(0, BlockContext);
	}
	public COMMA(): TerminalNode[];
	public COMMA(i: number): TerminalNode;
	public COMMA(i?: number): TerminalNode | TerminalNode[] {
		if (i === undefined) {
			return this.getTokens(ScssParser.COMMA);
		} else {
			return this.getToken(ScssParser.COMMA, i);
		}
	}
	constructor(parent: ParserRuleContext | undefined, invokingState: number) {
		super(parent, invokingState);
	}
	// @Override
	public get ruleIndex(): number { return ScssParser.RULE_eachDeclaration; }
	// @Override
	public enterRule(listener: ScssParserListener): void {
		if (listener.enterEachDeclaration) {
			listener.enterEachDeclaration(this);
		}
	}
	// @Override
	public exitRule(listener: ScssParserListener): void {
		if (listener.exitEachDeclaration) {
			listener.exitEachDeclaration(this);
		}
	}
	// @Override
	public accept<Result>(visitor: ScssParserVisitor<Result>): Result {
		if (visitor.visitEachDeclaration) {
			return visitor.visitEachDeclaration(this);
		} else {
			return visitor.visitChildren(this);
		}
	}
}


export class EachValueListContext extends ParserRuleContext {
	public Identifier(): TerminalNode[];
	public Identifier(i: number): TerminalNode;
	public Identifier(i?: number): TerminalNode | TerminalNode[] {
		if (i === undefined) {
			return this.getTokens(ScssParser.Identifier);
		} else {
			return this.getToken(ScssParser.Identifier, i);
		}
	}
	public COMMA(): TerminalNode[];
	public COMMA(i: number): TerminalNode;
	public COMMA(i?: number): TerminalNode | TerminalNode[] {
		if (i === undefined) {
			return this.getTokens(ScssParser.COMMA);
		} else {
			return this.getToken(ScssParser.COMMA, i);
		}
	}
	public identifierListOrMap(): IdentifierListOrMapContext[];
	public identifierListOrMap(i: number): IdentifierListOrMapContext;
	public identifierListOrMap(i?: number): IdentifierListOrMapContext | IdentifierListOrMapContext[] {
		if (i === undefined) {
			return this.getRuleContexts(IdentifierListOrMapContext);
		} else {
			return this.getRuleContext(i, IdentifierListOrMapContext);
		}
	}
	constructor(parent: ParserRuleContext | undefined, invokingState: number) {
		super(parent, invokingState);
	}
	// @Override
	public get ruleIndex(): number { return ScssParser.RULE_eachValueList; }
	// @Override
	public enterRule(listener: ScssParserListener): void {
		if (listener.enterEachValueList) {
			listener.enterEachValueList(this);
		}
	}
	// @Override
	public exitRule(listener: ScssParserListener): void {
		if (listener.exitEachValueList) {
			listener.exitEachValueList(this);
		}
	}
	// @Override
	public accept<Result>(visitor: ScssParserVisitor<Result>): Result {
		if (visitor.visitEachValueList) {
			return visitor.visitEachValueList(this);
		} else {
			return visitor.visitChildren(this);
		}
	}
}


export class IdentifierListOrMapContext extends ParserRuleContext {
	public LPAREN(): TerminalNode { return this.getToken(ScssParser.LPAREN, 0); }
	public identifierValue(): IdentifierValueContext[];
	public identifierValue(i: number): IdentifierValueContext;
	public identifierValue(i?: number): IdentifierValueContext | IdentifierValueContext[] {
		if (i === undefined) {
			return this.getRuleContexts(IdentifierValueContext);
		} else {
			return this.getRuleContext(i, IdentifierValueContext);
		}
	}
	public RPAREN(): TerminalNode { return this.getToken(ScssParser.RPAREN, 0); }
	public COMMA(): TerminalNode[];
	public COMMA(i: number): TerminalNode;
	public COMMA(i?: number): TerminalNode | TerminalNode[] {
		if (i === undefined) {
			return this.getTokens(ScssParser.COMMA);
		} else {
			return this.getToken(ScssParser.COMMA, i);
		}
	}
	constructor(parent: ParserRuleContext | undefined, invokingState: number) {
		super(parent, invokingState);
	}
	// @Override
	public get ruleIndex(): number { return ScssParser.RULE_identifierListOrMap; }
	// @Override
	public enterRule(listener: ScssParserListener): void {
		if (listener.enterIdentifierListOrMap) {
			listener.enterIdentifierListOrMap(this);
		}
	}
	// @Override
	public exitRule(listener: ScssParserListener): void {
		if (listener.exitIdentifierListOrMap) {
			listener.exitIdentifierListOrMap(this);
		}
	}
	// @Override
	public accept<Result>(visitor: ScssParserVisitor<Result>): Result {
		if (visitor.visitIdentifierListOrMap) {
			return visitor.visitIdentifierListOrMap(this);
		} else {
			return visitor.visitChildren(this);
		}
	}
}


export class IdentifierValueContext extends ParserRuleContext {
	public identifier(): IdentifierContext {
		return this.getRuleContext(0, IdentifierContext);
	}
	public COLON(): TerminalNode | undefined { return this.tryGetToken(ScssParser.COLON, 0); }
	public values(): ValuesContext | undefined {
		return this.tryGetRuleContext(0, ValuesContext);
	}
	constructor(parent: ParserRuleContext | undefined, invokingState: number) {
		super(parent, invokingState);
	}
	// @Override
	public get ruleIndex(): number { return ScssParser.RULE_identifierValue; }
	// @Override
	public enterRule(listener: ScssParserListener): void {
		if (listener.enterIdentifierValue) {
			listener.enterIdentifierValue(this);
		}
	}
	// @Override
	public exitRule(listener: ScssParserListener): void {
		if (listener.exitIdentifierValue) {
			listener.exitIdentifierValue(this);
		}
	}
	// @Override
	public accept<Result>(visitor: ScssParserVisitor<Result>): Result {
		if (visitor.visitIdentifierValue) {
			return visitor.visitIdentifierValue(this);
		} else {
			return visitor.visitChildren(this);
		}
	}
}


export class ImportDeclarationContext extends ParserRuleContext {
	public IMPORT(): TerminalNode { return this.getToken(ScssParser.IMPORT, 0); }
	public referenceUrl(): ReferenceUrlContext {
		return this.getRuleContext(0, ReferenceUrlContext);
	}
	public SEMI(): TerminalNode { return this.getToken(ScssParser.SEMI, 0); }
	public mediaTypes(): MediaTypesContext | undefined {
		return this.tryGetRuleContext(0, MediaTypesContext);
	}
	constructor(parent: ParserRuleContext | undefined, invokingState: number) {
		super(parent, invokingState);
	}
	// @Override
	public get ruleIndex(): number { return ScssParser.RULE_importDeclaration; }
	// @Override
	public enterRule(listener: ScssParserListener): void {
		if (listener.enterImportDeclaration) {
			listener.enterImportDeclaration(this);
		}
	}
	// @Override
	public exitRule(listener: ScssParserListener): void {
		if (listener.exitImportDeclaration) {
			listener.exitImportDeclaration(this);
		}
	}
	// @Override
	public accept<Result>(visitor: ScssParserVisitor<Result>): Result {
		if (visitor.visitImportDeclaration) {
			return visitor.visitImportDeclaration(this);
		} else {
			return visitor.visitChildren(this);
		}
	}
}


export class ReferenceUrlContext extends ParserRuleContext {
	public StringLiteral(): TerminalNode | undefined { return this.tryGetToken(ScssParser.StringLiteral, 0); }
	public UrlStart(): TerminalNode | undefined { return this.tryGetToken(ScssParser.UrlStart, 0); }
	public Url(): TerminalNode | undefined { return this.tryGetToken(ScssParser.Url, 0); }
	public UrlEnd(): TerminalNode | undefined { return this.tryGetToken(ScssParser.UrlEnd, 0); }
	constructor(parent: ParserRuleContext | undefined, invokingState: number) {
		super(parent, invokingState);
	}
	// @Override
	public get ruleIndex(): number { return ScssParser.RULE_referenceUrl; }
	// @Override
	public enterRule(listener: ScssParserListener): void {
		if (listener.enterReferenceUrl) {
			listener.enterReferenceUrl(this);
		}
	}
	// @Override
	public exitRule(listener: ScssParserListener): void {
		if (listener.exitReferenceUrl) {
			listener.exitReferenceUrl(this);
		}
	}
	// @Override
	public accept<Result>(visitor: ScssParserVisitor<Result>): Result {
		if (visitor.visitReferenceUrl) {
			return visitor.visitReferenceUrl(this);
		} else {
			return visitor.visitChildren(this);
		}
	}
}


export class MediaTypesContext extends ParserRuleContext {
	public Identifier(): TerminalNode[];
	public Identifier(i: number): TerminalNode;
	public Identifier(i?: number): TerminalNode | TerminalNode[] {
		if (i === undefined) {
			return this.getTokens(ScssParser.Identifier);
		} else {
			return this.getToken(ScssParser.Identifier, i);
		}
	}
	public COMMA(): TerminalNode[];
	public COMMA(i: number): TerminalNode;
	public COMMA(i?: number): TerminalNode | TerminalNode[] {
		if (i === undefined) {
			return this.getTokens(ScssParser.COMMA);
		} else {
			return this.getToken(ScssParser.COMMA, i);
		}
	}
	constructor(parent: ParserRuleContext | undefined, invokingState: number) {
		super(parent, invokingState);
	}
	// @Override
	public get ruleIndex(): number { return ScssParser.RULE_mediaTypes; }
	// @Override
	public enterRule(listener: ScssParserListener): void {
		if (listener.enterMediaTypes) {
			listener.enterMediaTypes(this);
		}
	}
	// @Override
	public exitRule(listener: ScssParserListener): void {
		if (listener.exitMediaTypes) {
			listener.exitMediaTypes(this);
		}
	}
	// @Override
	public accept<Result>(visitor: ScssParserVisitor<Result>): Result {
		if (visitor.visitMediaTypes) {
			return visitor.visitMediaTypes(this);
		} else {
			return visitor.visitChildren(this);
		}
	}
}


export class NestedContext extends ParserRuleContext {
	public AT(): TerminalNode { return this.getToken(ScssParser.AT, 0); }
	public nest(): NestContext {
		return this.getRuleContext(0, NestContext);
	}
	public selectors(): SelectorsContext {
		return this.getRuleContext(0, SelectorsContext);
	}
	public BlockStart(): TerminalNode { return this.getToken(ScssParser.BlockStart, 0); }
	public stylesheet(): StylesheetContext {
		return this.getRuleContext(0, StylesheetContext);
	}
	public BlockEnd(): TerminalNode { return this.getToken(ScssParser.BlockEnd, 0); }
	constructor(parent: ParserRuleContext | undefined, invokingState: number) {
		super(parent, invokingState);
	}
	// @Override
	public get ruleIndex(): number { return ScssParser.RULE_nested; }
	// @Override
	public enterRule(listener: ScssParserListener): void {
		if (listener.enterNested) {
			listener.enterNested(this);
		}
	}
	// @Override
	public exitRule(listener: ScssParserListener): void {
		if (listener.exitNested) {
			listener.exitNested(this);
		}
	}
	// @Override
	public accept<Result>(visitor: ScssParserVisitor<Result>): Result {
		if (visitor.visitNested) {
			return visitor.visitNested(this);
		} else {
			return visitor.visitChildren(this);
		}
	}
}


export class NestContext extends ParserRuleContext {
	public Identifier(): TerminalNode[];
	public Identifier(i: number): TerminalNode;
	public Identifier(i?: number): TerminalNode | TerminalNode[] {
		if (i === undefined) {
			return this.getTokens(ScssParser.Identifier);
		} else {
			return this.getToken(ScssParser.Identifier, i);
		}
	}
	public AND(): TerminalNode | undefined { return this.tryGetToken(ScssParser.AND, 0); }
	public pseudo(): PseudoContext[];
	public pseudo(i: number): PseudoContext;
	public pseudo(i?: number): PseudoContext | PseudoContext[] {
		if (i === undefined) {
			return this.getRuleContexts(PseudoContext);
		} else {
			return this.getRuleContext(i, PseudoContext);
		}
	}
	constructor(parent: ParserRuleContext | undefined, invokingState: number) {
		super(parent, invokingState);
	}
	// @Override
	public get ruleIndex(): number { return ScssParser.RULE_nest; }
	// @Override
	public enterRule(listener: ScssParserListener): void {
		if (listener.enterNest) {
			listener.enterNest(this);
		}
	}
	// @Override
	public exitRule(listener: ScssParserListener): void {
		if (listener.exitNest) {
			listener.exitNest(this);
		}
	}
	// @Override
	public accept<Result>(visitor: ScssParserVisitor<Result>): Result {
		if (visitor.visitNest) {
			return visitor.visitNest(this);
		} else {
			return visitor.visitChildren(this);
		}
	}
}


export class RulesetContext extends ParserRuleContext {
	public selectors(): SelectorsContext {
		return this.getRuleContext(0, SelectorsContext);
	}
	public block(): BlockContext {
		return this.getRuleContext(0, BlockContext);
	}
	constructor(parent: ParserRuleContext | undefined, invokingState: number) {
		super(parent, invokingState);
	}
	// @Override
	public get ruleIndex(): number { return ScssParser.RULE_ruleset; }
	// @Override
	public enterRule(listener: ScssParserListener): void {
		if (listener.enterRuleset) {
			listener.enterRuleset(this);
		}
	}
	// @Override
	public exitRule(listener: ScssParserListener): void {
		if (listener.exitRuleset) {
			listener.exitRuleset(this);
		}
	}
	// @Override
	public accept<Result>(visitor: ScssParserVisitor<Result>): Result {
		if (visitor.visitRuleset) {
			return visitor.visitRuleset(this);
		} else {
			return visitor.visitChildren(this);
		}
	}
}


export class BlockContext extends ParserRuleContext {
	public BlockStart(): TerminalNode { return this.getToken(ScssParser.BlockStart, 0); }
	public BlockEnd(): TerminalNode { return this.getToken(ScssParser.BlockEnd, 0); }
	public property(): PropertyContext[];
	public property(i: number): PropertyContext;
	public property(i?: number): PropertyContext | PropertyContext[] {
		if (i === undefined) {
			return this.getRuleContexts(PropertyContext);
		} else {
			return this.getRuleContext(i, PropertyContext);
		}
	}
	public SEMI(): TerminalNode[];
	public SEMI(i: number): TerminalNode;
	public SEMI(i?: number): TerminalNode | TerminalNode[] {
		if (i === undefined) {
			return this.getTokens(ScssParser.SEMI);
		} else {
			return this.getToken(ScssParser.SEMI, i);
		}
	}
	public statement(): StatementContext[];
	public statement(i: number): StatementContext;
	public statement(i?: number): StatementContext | StatementContext[] {
		if (i === undefined) {
			return this.getRuleContexts(StatementContext);
		} else {
			return this.getRuleContext(i, StatementContext);
		}
	}
	constructor(parent: ParserRuleContext | undefined, invokingState: number) {
		super(parent, invokingState);
	}
	// @Override
	public get ruleIndex(): number { return ScssParser.RULE_block; }
	// @Override
	public enterRule(listener: ScssParserListener): void {
		if (listener.enterBlock) {
			listener.enterBlock(this);
		}
	}
	// @Override
	public exitRule(listener: ScssParserListener): void {
		if (listener.exitBlock) {
			listener.exitBlock(this);
		}
	}
	// @Override
	public accept<Result>(visitor: ScssParserVisitor<Result>): Result {
		if (visitor.visitBlock) {
			return visitor.visitBlock(this);
		} else {
			return visitor.visitChildren(this);
		}
	}
}


export class SelectorsContext extends ParserRuleContext {
	public selector(): SelectorContext[];
	public selector(i: number): SelectorContext;
	public selector(i?: number): SelectorContext | SelectorContext[] {
		if (i === undefined) {
			return this.getRuleContexts(SelectorContext);
		} else {
			return this.getRuleContext(i, SelectorContext);
		}
	}
	public COMMA(): TerminalNode[];
	public COMMA(i: number): TerminalNode;
	public COMMA(i?: number): TerminalNode | TerminalNode[] {
		if (i === undefined) {
			return this.getTokens(ScssParser.COMMA);
		} else {
			return this.getToken(ScssParser.COMMA, i);
		}
	}
	constructor(parent: ParserRuleContext | undefined, invokingState: number) {
		super(parent, invokingState);
	}
	// @Override
	public get ruleIndex(): number { return ScssParser.RULE_selectors; }
	// @Override
	public enterRule(listener: ScssParserListener): void {
		if (listener.enterSelectors) {
			listener.enterSelectors(this);
		}
	}
	// @Override
	public exitRule(listener: ScssParserListener): void {
		if (listener.exitSelectors) {
			listener.exitSelectors(this);
		}
	}
	// @Override
	public accept<Result>(visitor: ScssParserVisitor<Result>): Result {
		if (visitor.visitSelectors) {
			return visitor.visitSelectors(this);
		} else {
			return visitor.visitChildren(this);
		}
	}
}


export class SelectorContext extends ParserRuleContext {
	public element(): ElementContext[];
	public element(i: number): ElementContext;
	public element(i?: number): ElementContext | ElementContext[] {
		if (i === undefined) {
			return this.getRuleContexts(ElementContext);
		} else {
			return this.getRuleContext(i, ElementContext);
		}
	}
	public selectorPrefix(): SelectorPrefixContext[];
	public selectorPrefix(i: number): SelectorPrefixContext;
	public selectorPrefix(i?: number): SelectorPrefixContext | SelectorPrefixContext[] {
		if (i === undefined) {
			return this.getRuleContexts(SelectorPrefixContext);
		} else {
			return this.getRuleContext(i, SelectorPrefixContext);
		}
	}
	public attrib(): AttribContext[];
	public attrib(i: number): AttribContext;
	public attrib(i?: number): AttribContext | AttribContext[] {
		if (i === undefined) {
			return this.getRuleContexts(AttribContext);
		} else {
			return this.getRuleContext(i, AttribContext);
		}
	}
	public pseudo(): PseudoContext | undefined {
		return this.tryGetRuleContext(0, PseudoContext);
	}
	constructor(parent: ParserRuleContext | undefined, invokingState: number) {
		super(parent, invokingState);
	}
	// @Override
	public get ruleIndex(): number { return ScssParser.RULE_selector; }
	// @Override
	public enterRule(listener: ScssParserListener): void {
		if (listener.enterSelector) {
			listener.enterSelector(this);
		}
	}
	// @Override
	public exitRule(listener: ScssParserListener): void {
		if (listener.exitSelector) {
			listener.exitSelector(this);
		}
	}
	// @Override
	public accept<Result>(visitor: ScssParserVisitor<Result>): Result {
		if (visitor.visitSelector) {
			return visitor.visitSelector(this);
		} else {
			return visitor.visitChildren(this);
		}
	}
}


export class SelectorPrefixContext extends ParserRuleContext {
	public GT(): TerminalNode | undefined { return this.tryGetToken(ScssParser.GT, 0); }
	public PLUS(): TerminalNode | undefined { return this.tryGetToken(ScssParser.PLUS, 0); }
	public TIL(): TerminalNode | undefined { return this.tryGetToken(ScssParser.TIL, 0); }
	constructor(parent: ParserRuleContext | undefined, invokingState: number) {
		super(parent, invokingState);
	}
	// @Override
	public get ruleIndex(): number { return ScssParser.RULE_selectorPrefix; }
	// @Override
	public enterRule(listener: ScssParserListener): void {
		if (listener.enterSelectorPrefix) {
			listener.enterSelectorPrefix(this);
		}
	}
	// @Override
	public exitRule(listener: ScssParserListener): void {
		if (listener.exitSelectorPrefix) {
			listener.exitSelectorPrefix(this);
		}
	}
	// @Override
	public accept<Result>(visitor: ScssParserVisitor<Result>): Result {
		if (visitor.visitSelectorPrefix) {
			return visitor.visitSelectorPrefix(this);
		} else {
			return visitor.visitChildren(this);
		}
	}
}


export class ElementContext extends ParserRuleContext {
	public identifier(): IdentifierContext | undefined {
		return this.tryGetRuleContext(0, IdentifierContext);
	}
	public HASH(): TerminalNode | undefined { return this.tryGetToken(ScssParser.HASH, 0); }
	public DOT(): TerminalNode | undefined { return this.tryGetToken(ScssParser.DOT, 0); }
	public AND(): TerminalNode | undefined { return this.tryGetToken(ScssParser.AND, 0); }
	public TIMES(): TerminalNode | undefined { return this.tryGetToken(ScssParser.TIMES, 0); }
	constructor(parent: ParserRuleContext | undefined, invokingState: number) {
		super(parent, invokingState);
	}
	// @Override
	public get ruleIndex(): number { return ScssParser.RULE_element; }
	// @Override
	public enterRule(listener: ScssParserListener): void {
		if (listener.enterElement) {
			listener.enterElement(this);
		}
	}
	// @Override
	public exitRule(listener: ScssParserListener): void {
		if (listener.exitElement) {
			listener.exitElement(this);
		}
	}
	// @Override
	public accept<Result>(visitor: ScssParserVisitor<Result>): Result {
		if (visitor.visitElement) {
			return visitor.visitElement(this);
		} else {
			return visitor.visitChildren(this);
		}
	}
}


export class PseudoContext extends ParserRuleContext {
	public Identifier(): TerminalNode | undefined { return this.tryGetToken(ScssParser.Identifier, 0); }
	public COLON(): TerminalNode | undefined { return this.tryGetToken(ScssParser.COLON, 0); }
	public COLONCOLON(): TerminalNode | undefined { return this.tryGetToken(ScssParser.COLONCOLON, 0); }
	public functionCall(): FunctionCallContext | undefined {
		return this.tryGetRuleContext(0, FunctionCallContext);
	}
	constructor(parent: ParserRuleContext | undefined, invokingState: number) {
		super(parent, invokingState);
	}
	// @Override
	public get ruleIndex(): number { return ScssParser.RULE_pseudo; }
	// @Override
	public enterRule(listener: ScssParserListener): void {
		if (listener.enterPseudo) {
			listener.enterPseudo(this);
		}
	}
	// @Override
	public exitRule(listener: ScssParserListener): void {
		if (listener.exitPseudo) {
			listener.exitPseudo(this);
		}
	}
	// @Override
	public accept<Result>(visitor: ScssParserVisitor<Result>): Result {
		if (visitor.visitPseudo) {
			return visitor.visitPseudo(this);
		} else {
			return visitor.visitChildren(this);
		}
	}
}


export class AttribContext extends ParserRuleContext {
	public LBRACK(): TerminalNode { return this.getToken(ScssParser.LBRACK, 0); }
	public Identifier(): TerminalNode[];
	public Identifier(i: number): TerminalNode;
	public Identifier(i?: number): TerminalNode | TerminalNode[] {
		if (i === undefined) {
			return this.getTokens(ScssParser.Identifier);
		} else {
			return this.getToken(ScssParser.Identifier, i);
		}
	}
	public RBRACK(): TerminalNode { return this.getToken(ScssParser.RBRACK, 0); }
	public attribRelate(): AttribRelateContext | undefined {
		return this.tryGetRuleContext(0, AttribRelateContext);
	}
	public StringLiteral(): TerminalNode | undefined { return this.tryGetToken(ScssParser.StringLiteral, 0); }
	constructor(parent: ParserRuleContext | undefined, invokingState: number) {
		super(parent, invokingState);
	}
	// @Override
	public get ruleIndex(): number { return ScssParser.RULE_attrib; }
	// @Override
	public enterRule(listener: ScssParserListener): void {
		if (listener.enterAttrib) {
			listener.enterAttrib(this);
		}
	}
	// @Override
	public exitRule(listener: ScssParserListener): void {
		if (listener.exitAttrib) {
			listener.exitAttrib(this);
		}
	}
	// @Override
	public accept<Result>(visitor: ScssParserVisitor<Result>): Result {
		if (visitor.visitAttrib) {
			return visitor.visitAttrib(this);
		} else {
			return visitor.visitChildren(this);
		}
	}
}


export class AttribRelateContext extends ParserRuleContext {
	public EQ(): TerminalNode | undefined { return this.tryGetToken(ScssParser.EQ, 0); }
	public TILD_EQ(): TerminalNode | undefined { return this.tryGetToken(ScssParser.TILD_EQ, 0); }
	public PIPE_EQ(): TerminalNode | undefined { return this.tryGetToken(ScssParser.PIPE_EQ, 0); }
	constructor(parent: ParserRuleContext | undefined, invokingState: number) {
		super(parent, invokingState);
	}
	// @Override
	public get ruleIndex(): number { return ScssParser.RULE_attribRelate; }
	// @Override
	public enterRule(listener: ScssParserListener): void {
		if (listener.enterAttribRelate) {
			listener.enterAttribRelate(this);
		}
	}
	// @Override
	public exitRule(listener: ScssParserListener): void {
		if (listener.exitAttribRelate) {
			listener.exitAttribRelate(this);
		}
	}
	// @Override
	public accept<Result>(visitor: ScssParserVisitor<Result>): Result {
		if (visitor.visitAttribRelate) {
			return visitor.visitAttribRelate(this);
		} else {
			return visitor.visitChildren(this);
		}
	}
}


export class IdentifierContext extends ParserRuleContext {
	public Identifier(): TerminalNode | undefined { return this.tryGetToken(ScssParser.Identifier, 0); }
	public identifierPart(): IdentifierPartContext[];
	public identifierPart(i: number): IdentifierPartContext;
	public identifierPart(i?: number): IdentifierPartContext | IdentifierPartContext[] {
		if (i === undefined) {
			return this.getRuleContexts(IdentifierPartContext);
		} else {
			return this.getRuleContext(i, IdentifierPartContext);
		}
	}
	public InterpolationStart(): TerminalNode | undefined { return this.tryGetToken(ScssParser.InterpolationStart, 0); }
	public identifierVariableName(): IdentifierVariableNameContext | undefined {
		return this.tryGetRuleContext(0, IdentifierVariableNameContext);
	}
	public BlockEnd(): TerminalNode | undefined { return this.tryGetToken(ScssParser.BlockEnd, 0); }
	constructor(parent: ParserRuleContext | undefined, invokingState: number) {
		super(parent, invokingState);
	}
	// @Override
	public get ruleIndex(): number { return ScssParser.RULE_identifier; }
	// @Override
	public enterRule(listener: ScssParserListener): void {
		if (listener.enterIdentifier) {
			listener.enterIdentifier(this);
		}
	}
	// @Override
	public exitRule(listener: ScssParserListener): void {
		if (listener.exitIdentifier) {
			listener.exitIdentifier(this);
		}
	}
	// @Override
	public accept<Result>(visitor: ScssParserVisitor<Result>): Result {
		if (visitor.visitIdentifier) {
			return visitor.visitIdentifier(this);
		} else {
			return visitor.visitChildren(this);
		}
	}
}


export class IdentifierPartContext extends ParserRuleContext {
	public InterpolationStartAfter(): TerminalNode | undefined { return this.tryGetToken(ScssParser.InterpolationStartAfter, 0); }
	public identifierVariableName(): IdentifierVariableNameContext | undefined {
		return this.tryGetRuleContext(0, IdentifierVariableNameContext);
	}
	public BlockEnd(): TerminalNode | undefined { return this.tryGetToken(ScssParser.BlockEnd, 0); }
	public IdentifierAfter(): TerminalNode | undefined { return this.tryGetToken(ScssParser.IdentifierAfter, 0); }
	constructor(parent: ParserRuleContext | undefined, invokingState: number) {
		super(parent, invokingState);
	}
	// @Override
	public get ruleIndex(): number { return ScssParser.RULE_identifierPart; }
	// @Override
	public enterRule(listener: ScssParserListener): void {
		if (listener.enterIdentifierPart) {
			listener.enterIdentifierPart(this);
		}
	}
	// @Override
	public exitRule(listener: ScssParserListener): void {
		if (listener.exitIdentifierPart) {
			listener.exitIdentifierPart(this);
		}
	}
	// @Override
	public accept<Result>(visitor: ScssParserVisitor<Result>): Result {
		if (visitor.visitIdentifierPart) {
			return visitor.visitIdentifierPart(this);
		} else {
			return visitor.visitChildren(this);
		}
	}
}


export class IdentifierVariableNameContext extends ParserRuleContext {
	public DOLLAR(): TerminalNode { return this.getToken(ScssParser.DOLLAR, 0); }
	public Identifier(): TerminalNode | undefined { return this.tryGetToken(ScssParser.Identifier, 0); }
	public IdentifierAfter(): TerminalNode | undefined { return this.tryGetToken(ScssParser.IdentifierAfter, 0); }
	constructor(parent: ParserRuleContext | undefined, invokingState: number) {
		super(parent, invokingState);
	}
	// @Override
	public get ruleIndex(): number { return ScssParser.RULE_identifierVariableName; }
	// @Override
	public enterRule(listener: ScssParserListener): void {
		if (listener.enterIdentifierVariableName) {
			listener.enterIdentifierVariableName(this);
		}
	}
	// @Override
	public exitRule(listener: ScssParserListener): void {
		if (listener.exitIdentifierVariableName) {
			listener.exitIdentifierVariableName(this);
		}
	}
	// @Override
	public accept<Result>(visitor: ScssParserVisitor<Result>): Result {
		if (visitor.visitIdentifierVariableName) {
			return visitor.visitIdentifierVariableName(this);
		} else {
			return visitor.visitChildren(this);
		}
	}
}


export class PropertyContext extends ParserRuleContext {
	public identifier(): IdentifierContext {
		return this.getRuleContext(0, IdentifierContext);
	}
	public COLON(): TerminalNode { return this.getToken(ScssParser.COLON, 0); }
	public values(): ValuesContext {
		return this.getRuleContext(0, ValuesContext);
	}
	constructor(parent: ParserRuleContext | undefined, invokingState: number) {
		super(parent, invokingState);
	}
	// @Override
	public get ruleIndex(): number { return ScssParser.RULE_property; }
	// @Override
	public enterRule(listener: ScssParserListener): void {
		if (listener.enterProperty) {
			listener.enterProperty(this);
		}
	}
	// @Override
	public exitRule(listener: ScssParserListener): void {
		if (listener.exitProperty) {
			listener.exitProperty(this);
		}
	}
	// @Override
	public accept<Result>(visitor: ScssParserVisitor<Result>): Result {
		if (visitor.visitProperty) {
			return visitor.visitProperty(this);
		} else {
			return visitor.visitChildren(this);
		}
	}
}


export class ValuesContext extends ParserRuleContext {
	public commandStatement(): CommandStatementContext[];
	public commandStatement(i: number): CommandStatementContext;
	public commandStatement(i?: number): CommandStatementContext | CommandStatementContext[] {
		if (i === undefined) {
			return this.getRuleContexts(CommandStatementContext);
		} else {
			return this.getRuleContext(i, CommandStatementContext);
		}
	}
	public COMMA(): TerminalNode[];
	public COMMA(i: number): TerminalNode;
	public COMMA(i?: number): TerminalNode | TerminalNode[] {
		if (i === undefined) {
			return this.getTokens(ScssParser.COMMA);
		} else {
			return this.getToken(ScssParser.COMMA, i);
		}
	}
	constructor(parent: ParserRuleContext | undefined, invokingState: number) {
		super(parent, invokingState);
	}
	// @Override
	public get ruleIndex(): number { return ScssParser.RULE_values; }
	// @Override
	public enterRule(listener: ScssParserListener): void {
		if (listener.enterValues) {
			listener.enterValues(this);
		}
	}
	// @Override
	public exitRule(listener: ScssParserListener): void {
		if (listener.exitValues) {
			listener.exitValues(this);
		}
	}
	// @Override
	public accept<Result>(visitor: ScssParserVisitor<Result>): Result {
		if (visitor.visitValues) {
			return visitor.visitValues(this);
		} else {
			return visitor.visitChildren(this);
		}
	}
}


export class UrlContext extends ParserRuleContext {
	public UrlStart(): TerminalNode { return this.getToken(ScssParser.UrlStart, 0); }
	public Url(): TerminalNode { return this.getToken(ScssParser.Url, 0); }
	public UrlEnd(): TerminalNode { return this.getToken(ScssParser.UrlEnd, 0); }
	constructor(parent: ParserRuleContext | undefined, invokingState: number) {
		super(parent, invokingState);
	}
	// @Override
	public get ruleIndex(): number { return ScssParser.RULE_url; }
	// @Override
	public enterRule(listener: ScssParserListener): void {
		if (listener.enterUrl) {
			listener.enterUrl(this);
		}
	}
	// @Override
	public exitRule(listener: ScssParserListener): void {
		if (listener.exitUrl) {
			listener.exitUrl(this);
		}
	}
	// @Override
	public accept<Result>(visitor: ScssParserVisitor<Result>): Result {
		if (visitor.visitUrl) {
			return visitor.visitUrl(this);
		} else {
			return visitor.visitChildren(this);
		}
	}
}


export class MeasurementContext extends ParserRuleContext {
	public Number(): TerminalNode { return this.getToken(ScssParser.Number, 0); }
	public Unit(): TerminalNode | undefined { return this.tryGetToken(ScssParser.Unit, 0); }
	constructor(parent: ParserRuleContext | undefined, invokingState: number) {
		super(parent, invokingState);
	}
	// @Override
	public get ruleIndex(): number { return ScssParser.RULE_measurement; }
	// @Override
	public enterRule(listener: ScssParserListener): void {
		if (listener.enterMeasurement) {
			listener.enterMeasurement(this);
		}
	}
	// @Override
	public exitRule(listener: ScssParserListener): void {
		if (listener.exitMeasurement) {
			listener.exitMeasurement(this);
		}
	}
	// @Override
	public accept<Result>(visitor: ScssParserVisitor<Result>): Result {
		if (visitor.visitMeasurement) {
			return visitor.visitMeasurement(this);
		} else {
			return visitor.visitChildren(this);
		}
	}
}


export class FunctionCallContext extends ParserRuleContext {
	public Identifier(): TerminalNode { return this.getToken(ScssParser.Identifier, 0); }
	public LPAREN(): TerminalNode { return this.getToken(ScssParser.LPAREN, 0); }
	public RPAREN(): TerminalNode { return this.getToken(ScssParser.RPAREN, 0); }
	public values(): ValuesContext | undefined {
		return this.tryGetRuleContext(0, ValuesContext);
	}
	constructor(parent: ParserRuleContext | undefined, invokingState: number) {
		super(parent, invokingState);
	}
	// @Override
	public get ruleIndex(): number { return ScssParser.RULE_functionCall; }
	// @Override
	public enterRule(listener: ScssParserListener): void {
		if (listener.enterFunctionCall) {
			listener.enterFunctionCall(this);
		}
	}
	// @Override
	public exitRule(listener: ScssParserListener): void {
		if (listener.exitFunctionCall) {
			listener.exitFunctionCall(this);
		}
	}
	// @Override
	public accept<Result>(visitor: ScssParserVisitor<Result>): Result {
		if (visitor.visitFunctionCall) {
			return visitor.visitFunctionCall(this);
		} else {
			return visitor.visitChildren(this);
		}
	}
}


