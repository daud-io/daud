// Generated from src/parser/ScssParser.g4 by ANTLR 4.7.3-SNAPSHOT


import { ParseTreeListener } from "antlr4ts/tree/ParseTreeListener";

import { StylesheetContext } from "./ScssParser";
import { StatementContext } from "./ScssParser";
import { ParamsContext } from "./ScssParser";
import { ParamContext } from "./ScssParser";
import { VariableNameContext } from "./ScssParser";
import { ParamOptionalValueContext } from "./ScssParser";
import { MixinDeclarationContext } from "./ScssParser";
import { IncludeDeclarationContext } from "./ScssParser";
import { FunctionDeclarationContext } from "./ScssParser";
import { FunctionBodyContext } from "./ScssParser";
import { FunctionReturnContext } from "./ScssParser";
import { FunctionStatementContext } from "./ScssParser";
import { CommandStatementContext } from "./ScssParser";
import { MathCharacterContext } from "./ScssParser";
import { MathStatementContext } from "./ScssParser";
import { ExpressionContext } from "./ScssParser";
import { IfDeclarationContext } from "./ScssParser";
import { ElseIfStatementContext } from "./ScssParser";
import { ElseStatementContext } from "./ScssParser";
import { ConditionsContext } from "./ScssParser";
import { ConditionContext } from "./ScssParser";
import { VariableDeclarationContext } from "./ScssParser";
import { ForDeclarationContext } from "./ScssParser";
import { FromNumberContext } from "./ScssParser";
import { ThroughNumberContext } from "./ScssParser";
import { WhileDeclarationContext } from "./ScssParser";
import { EachDeclarationContext } from "./ScssParser";
import { EachValueListContext } from "./ScssParser";
import { IdentifierListOrMapContext } from "./ScssParser";
import { IdentifierValueContext } from "./ScssParser";
import { ImportDeclarationContext } from "./ScssParser";
import { ReferenceUrlContext } from "./ScssParser";
import { MediaTypesContext } from "./ScssParser";
import { NestedContext } from "./ScssParser";
import { NestContext } from "./ScssParser";
import { RulesetContext } from "./ScssParser";
import { BlockContext } from "./ScssParser";
import { SelectorsContext } from "./ScssParser";
import { SelectorContext } from "./ScssParser";
import { SelectorPrefixContext } from "./ScssParser";
import { ElementContext } from "./ScssParser";
import { PseudoContext } from "./ScssParser";
import { AttribContext } from "./ScssParser";
import { AttribRelateContext } from "./ScssParser";
import { IdentifierContext } from "./ScssParser";
import { IdentifierPartContext } from "./ScssParser";
import { IdentifierVariableNameContext } from "./ScssParser";
import { PropertyContext } from "./ScssParser";
import { ValuesContext } from "./ScssParser";
import { UrlContext } from "./ScssParser";
import { MeasurementContext } from "./ScssParser";
import { FunctionCallContext } from "./ScssParser";


/**
 * This interface defines a complete listener for a parse tree produced by
 * `ScssParser`.
 */
export interface ScssParserListener extends ParseTreeListener {
	/**
	 * Enter a parse tree produced by `ScssParser.stylesheet`.
	 * @param ctx the parse tree
	 */
	enterStylesheet?: (ctx: StylesheetContext) => void;
	/**
	 * Exit a parse tree produced by `ScssParser.stylesheet`.
	 * @param ctx the parse tree
	 */
	exitStylesheet?: (ctx: StylesheetContext) => void;

	/**
	 * Enter a parse tree produced by `ScssParser.statement`.
	 * @param ctx the parse tree
	 */
	enterStatement?: (ctx: StatementContext) => void;
	/**
	 * Exit a parse tree produced by `ScssParser.statement`.
	 * @param ctx the parse tree
	 */
	exitStatement?: (ctx: StatementContext) => void;

	/**
	 * Enter a parse tree produced by `ScssParser.params`.
	 * @param ctx the parse tree
	 */
	enterParams?: (ctx: ParamsContext) => void;
	/**
	 * Exit a parse tree produced by `ScssParser.params`.
	 * @param ctx the parse tree
	 */
	exitParams?: (ctx: ParamsContext) => void;

	/**
	 * Enter a parse tree produced by `ScssParser.param`.
	 * @param ctx the parse tree
	 */
	enterParam?: (ctx: ParamContext) => void;
	/**
	 * Exit a parse tree produced by `ScssParser.param`.
	 * @param ctx the parse tree
	 */
	exitParam?: (ctx: ParamContext) => void;

	/**
	 * Enter a parse tree produced by `ScssParser.variableName`.
	 * @param ctx the parse tree
	 */
	enterVariableName?: (ctx: VariableNameContext) => void;
	/**
	 * Exit a parse tree produced by `ScssParser.variableName`.
	 * @param ctx the parse tree
	 */
	exitVariableName?: (ctx: VariableNameContext) => void;

	/**
	 * Enter a parse tree produced by `ScssParser.paramOptionalValue`.
	 * @param ctx the parse tree
	 */
	enterParamOptionalValue?: (ctx: ParamOptionalValueContext) => void;
	/**
	 * Exit a parse tree produced by `ScssParser.paramOptionalValue`.
	 * @param ctx the parse tree
	 */
	exitParamOptionalValue?: (ctx: ParamOptionalValueContext) => void;

	/**
	 * Enter a parse tree produced by `ScssParser.mixinDeclaration`.
	 * @param ctx the parse tree
	 */
	enterMixinDeclaration?: (ctx: MixinDeclarationContext) => void;
	/**
	 * Exit a parse tree produced by `ScssParser.mixinDeclaration`.
	 * @param ctx the parse tree
	 */
	exitMixinDeclaration?: (ctx: MixinDeclarationContext) => void;

	/**
	 * Enter a parse tree produced by `ScssParser.includeDeclaration`.
	 * @param ctx the parse tree
	 */
	enterIncludeDeclaration?: (ctx: IncludeDeclarationContext) => void;
	/**
	 * Exit a parse tree produced by `ScssParser.includeDeclaration`.
	 * @param ctx the parse tree
	 */
	exitIncludeDeclaration?: (ctx: IncludeDeclarationContext) => void;

	/**
	 * Enter a parse tree produced by `ScssParser.functionDeclaration`.
	 * @param ctx the parse tree
	 */
	enterFunctionDeclaration?: (ctx: FunctionDeclarationContext) => void;
	/**
	 * Exit a parse tree produced by `ScssParser.functionDeclaration`.
	 * @param ctx the parse tree
	 */
	exitFunctionDeclaration?: (ctx: FunctionDeclarationContext) => void;

	/**
	 * Enter a parse tree produced by `ScssParser.functionBody`.
	 * @param ctx the parse tree
	 */
	enterFunctionBody?: (ctx: FunctionBodyContext) => void;
	/**
	 * Exit a parse tree produced by `ScssParser.functionBody`.
	 * @param ctx the parse tree
	 */
	exitFunctionBody?: (ctx: FunctionBodyContext) => void;

	/**
	 * Enter a parse tree produced by `ScssParser.functionReturn`.
	 * @param ctx the parse tree
	 */
	enterFunctionReturn?: (ctx: FunctionReturnContext) => void;
	/**
	 * Exit a parse tree produced by `ScssParser.functionReturn`.
	 * @param ctx the parse tree
	 */
	exitFunctionReturn?: (ctx: FunctionReturnContext) => void;

	/**
	 * Enter a parse tree produced by `ScssParser.functionStatement`.
	 * @param ctx the parse tree
	 */
	enterFunctionStatement?: (ctx: FunctionStatementContext) => void;
	/**
	 * Exit a parse tree produced by `ScssParser.functionStatement`.
	 * @param ctx the parse tree
	 */
	exitFunctionStatement?: (ctx: FunctionStatementContext) => void;

	/**
	 * Enter a parse tree produced by `ScssParser.commandStatement`.
	 * @param ctx the parse tree
	 */
	enterCommandStatement?: (ctx: CommandStatementContext) => void;
	/**
	 * Exit a parse tree produced by `ScssParser.commandStatement`.
	 * @param ctx the parse tree
	 */
	exitCommandStatement?: (ctx: CommandStatementContext) => void;

	/**
	 * Enter a parse tree produced by `ScssParser.mathCharacter`.
	 * @param ctx the parse tree
	 */
	enterMathCharacter?: (ctx: MathCharacterContext) => void;
	/**
	 * Exit a parse tree produced by `ScssParser.mathCharacter`.
	 * @param ctx the parse tree
	 */
	exitMathCharacter?: (ctx: MathCharacterContext) => void;

	/**
	 * Enter a parse tree produced by `ScssParser.mathStatement`.
	 * @param ctx the parse tree
	 */
	enterMathStatement?: (ctx: MathStatementContext) => void;
	/**
	 * Exit a parse tree produced by `ScssParser.mathStatement`.
	 * @param ctx the parse tree
	 */
	exitMathStatement?: (ctx: MathStatementContext) => void;

	/**
	 * Enter a parse tree produced by `ScssParser.expression`.
	 * @param ctx the parse tree
	 */
	enterExpression?: (ctx: ExpressionContext) => void;
	/**
	 * Exit a parse tree produced by `ScssParser.expression`.
	 * @param ctx the parse tree
	 */
	exitExpression?: (ctx: ExpressionContext) => void;

	/**
	 * Enter a parse tree produced by `ScssParser.ifDeclaration`.
	 * @param ctx the parse tree
	 */
	enterIfDeclaration?: (ctx: IfDeclarationContext) => void;
	/**
	 * Exit a parse tree produced by `ScssParser.ifDeclaration`.
	 * @param ctx the parse tree
	 */
	exitIfDeclaration?: (ctx: IfDeclarationContext) => void;

	/**
	 * Enter a parse tree produced by `ScssParser.elseIfStatement`.
	 * @param ctx the parse tree
	 */
	enterElseIfStatement?: (ctx: ElseIfStatementContext) => void;
	/**
	 * Exit a parse tree produced by `ScssParser.elseIfStatement`.
	 * @param ctx the parse tree
	 */
	exitElseIfStatement?: (ctx: ElseIfStatementContext) => void;

	/**
	 * Enter a parse tree produced by `ScssParser.elseStatement`.
	 * @param ctx the parse tree
	 */
	enterElseStatement?: (ctx: ElseStatementContext) => void;
	/**
	 * Exit a parse tree produced by `ScssParser.elseStatement`.
	 * @param ctx the parse tree
	 */
	exitElseStatement?: (ctx: ElseStatementContext) => void;

	/**
	 * Enter a parse tree produced by `ScssParser.conditions`.
	 * @param ctx the parse tree
	 */
	enterConditions?: (ctx: ConditionsContext) => void;
	/**
	 * Exit a parse tree produced by `ScssParser.conditions`.
	 * @param ctx the parse tree
	 */
	exitConditions?: (ctx: ConditionsContext) => void;

	/**
	 * Enter a parse tree produced by `ScssParser.condition`.
	 * @param ctx the parse tree
	 */
	enterCondition?: (ctx: ConditionContext) => void;
	/**
	 * Exit a parse tree produced by `ScssParser.condition`.
	 * @param ctx the parse tree
	 */
	exitCondition?: (ctx: ConditionContext) => void;

	/**
	 * Enter a parse tree produced by `ScssParser.variableDeclaration`.
	 * @param ctx the parse tree
	 */
	enterVariableDeclaration?: (ctx: VariableDeclarationContext) => void;
	/**
	 * Exit a parse tree produced by `ScssParser.variableDeclaration`.
	 * @param ctx the parse tree
	 */
	exitVariableDeclaration?: (ctx: VariableDeclarationContext) => void;

	/**
	 * Enter a parse tree produced by `ScssParser.forDeclaration`.
	 * @param ctx the parse tree
	 */
	enterForDeclaration?: (ctx: ForDeclarationContext) => void;
	/**
	 * Exit a parse tree produced by `ScssParser.forDeclaration`.
	 * @param ctx the parse tree
	 */
	exitForDeclaration?: (ctx: ForDeclarationContext) => void;

	/**
	 * Enter a parse tree produced by `ScssParser.fromNumber`.
	 * @param ctx the parse tree
	 */
	enterFromNumber?: (ctx: FromNumberContext) => void;
	/**
	 * Exit a parse tree produced by `ScssParser.fromNumber`.
	 * @param ctx the parse tree
	 */
	exitFromNumber?: (ctx: FromNumberContext) => void;

	/**
	 * Enter a parse tree produced by `ScssParser.throughNumber`.
	 * @param ctx the parse tree
	 */
	enterThroughNumber?: (ctx: ThroughNumberContext) => void;
	/**
	 * Exit a parse tree produced by `ScssParser.throughNumber`.
	 * @param ctx the parse tree
	 */
	exitThroughNumber?: (ctx: ThroughNumberContext) => void;

	/**
	 * Enter a parse tree produced by `ScssParser.whileDeclaration`.
	 * @param ctx the parse tree
	 */
	enterWhileDeclaration?: (ctx: WhileDeclarationContext) => void;
	/**
	 * Exit a parse tree produced by `ScssParser.whileDeclaration`.
	 * @param ctx the parse tree
	 */
	exitWhileDeclaration?: (ctx: WhileDeclarationContext) => void;

	/**
	 * Enter a parse tree produced by `ScssParser.eachDeclaration`.
	 * @param ctx the parse tree
	 */
	enterEachDeclaration?: (ctx: EachDeclarationContext) => void;
	/**
	 * Exit a parse tree produced by `ScssParser.eachDeclaration`.
	 * @param ctx the parse tree
	 */
	exitEachDeclaration?: (ctx: EachDeclarationContext) => void;

	/**
	 * Enter a parse tree produced by `ScssParser.eachValueList`.
	 * @param ctx the parse tree
	 */
	enterEachValueList?: (ctx: EachValueListContext) => void;
	/**
	 * Exit a parse tree produced by `ScssParser.eachValueList`.
	 * @param ctx the parse tree
	 */
	exitEachValueList?: (ctx: EachValueListContext) => void;

	/**
	 * Enter a parse tree produced by `ScssParser.identifierListOrMap`.
	 * @param ctx the parse tree
	 */
	enterIdentifierListOrMap?: (ctx: IdentifierListOrMapContext) => void;
	/**
	 * Exit a parse tree produced by `ScssParser.identifierListOrMap`.
	 * @param ctx the parse tree
	 */
	exitIdentifierListOrMap?: (ctx: IdentifierListOrMapContext) => void;

	/**
	 * Enter a parse tree produced by `ScssParser.identifierValue`.
	 * @param ctx the parse tree
	 */
	enterIdentifierValue?: (ctx: IdentifierValueContext) => void;
	/**
	 * Exit a parse tree produced by `ScssParser.identifierValue`.
	 * @param ctx the parse tree
	 */
	exitIdentifierValue?: (ctx: IdentifierValueContext) => void;

	/**
	 * Enter a parse tree produced by `ScssParser.importDeclaration`.
	 * @param ctx the parse tree
	 */
	enterImportDeclaration?: (ctx: ImportDeclarationContext) => void;
	/**
	 * Exit a parse tree produced by `ScssParser.importDeclaration`.
	 * @param ctx the parse tree
	 */
	exitImportDeclaration?: (ctx: ImportDeclarationContext) => void;

	/**
	 * Enter a parse tree produced by `ScssParser.referenceUrl`.
	 * @param ctx the parse tree
	 */
	enterReferenceUrl?: (ctx: ReferenceUrlContext) => void;
	/**
	 * Exit a parse tree produced by `ScssParser.referenceUrl`.
	 * @param ctx the parse tree
	 */
	exitReferenceUrl?: (ctx: ReferenceUrlContext) => void;

	/**
	 * Enter a parse tree produced by `ScssParser.mediaTypes`.
	 * @param ctx the parse tree
	 */
	enterMediaTypes?: (ctx: MediaTypesContext) => void;
	/**
	 * Exit a parse tree produced by `ScssParser.mediaTypes`.
	 * @param ctx the parse tree
	 */
	exitMediaTypes?: (ctx: MediaTypesContext) => void;

	/**
	 * Enter a parse tree produced by `ScssParser.nested`.
	 * @param ctx the parse tree
	 */
	enterNested?: (ctx: NestedContext) => void;
	/**
	 * Exit a parse tree produced by `ScssParser.nested`.
	 * @param ctx the parse tree
	 */
	exitNested?: (ctx: NestedContext) => void;

	/**
	 * Enter a parse tree produced by `ScssParser.nest`.
	 * @param ctx the parse tree
	 */
	enterNest?: (ctx: NestContext) => void;
	/**
	 * Exit a parse tree produced by `ScssParser.nest`.
	 * @param ctx the parse tree
	 */
	exitNest?: (ctx: NestContext) => void;

	/**
	 * Enter a parse tree produced by `ScssParser.ruleset`.
	 * @param ctx the parse tree
	 */
	enterRuleset?: (ctx: RulesetContext) => void;
	/**
	 * Exit a parse tree produced by `ScssParser.ruleset`.
	 * @param ctx the parse tree
	 */
	exitRuleset?: (ctx: RulesetContext) => void;

	/**
	 * Enter a parse tree produced by `ScssParser.block`.
	 * @param ctx the parse tree
	 */
	enterBlock?: (ctx: BlockContext) => void;
	/**
	 * Exit a parse tree produced by `ScssParser.block`.
	 * @param ctx the parse tree
	 */
	exitBlock?: (ctx: BlockContext) => void;

	/**
	 * Enter a parse tree produced by `ScssParser.selectors`.
	 * @param ctx the parse tree
	 */
	enterSelectors?: (ctx: SelectorsContext) => void;
	/**
	 * Exit a parse tree produced by `ScssParser.selectors`.
	 * @param ctx the parse tree
	 */
	exitSelectors?: (ctx: SelectorsContext) => void;

	/**
	 * Enter a parse tree produced by `ScssParser.selector`.
	 * @param ctx the parse tree
	 */
	enterSelector?: (ctx: SelectorContext) => void;
	/**
	 * Exit a parse tree produced by `ScssParser.selector`.
	 * @param ctx the parse tree
	 */
	exitSelector?: (ctx: SelectorContext) => void;

	/**
	 * Enter a parse tree produced by `ScssParser.selectorPrefix`.
	 * @param ctx the parse tree
	 */
	enterSelectorPrefix?: (ctx: SelectorPrefixContext) => void;
	/**
	 * Exit a parse tree produced by `ScssParser.selectorPrefix`.
	 * @param ctx the parse tree
	 */
	exitSelectorPrefix?: (ctx: SelectorPrefixContext) => void;

	/**
	 * Enter a parse tree produced by `ScssParser.element`.
	 * @param ctx the parse tree
	 */
	enterElement?: (ctx: ElementContext) => void;
	/**
	 * Exit a parse tree produced by `ScssParser.element`.
	 * @param ctx the parse tree
	 */
	exitElement?: (ctx: ElementContext) => void;

	/**
	 * Enter a parse tree produced by `ScssParser.pseudo`.
	 * @param ctx the parse tree
	 */
	enterPseudo?: (ctx: PseudoContext) => void;
	/**
	 * Exit a parse tree produced by `ScssParser.pseudo`.
	 * @param ctx the parse tree
	 */
	exitPseudo?: (ctx: PseudoContext) => void;

	/**
	 * Enter a parse tree produced by `ScssParser.attrib`.
	 * @param ctx the parse tree
	 */
	enterAttrib?: (ctx: AttribContext) => void;
	/**
	 * Exit a parse tree produced by `ScssParser.attrib`.
	 * @param ctx the parse tree
	 */
	exitAttrib?: (ctx: AttribContext) => void;

	/**
	 * Enter a parse tree produced by `ScssParser.attribRelate`.
	 * @param ctx the parse tree
	 */
	enterAttribRelate?: (ctx: AttribRelateContext) => void;
	/**
	 * Exit a parse tree produced by `ScssParser.attribRelate`.
	 * @param ctx the parse tree
	 */
	exitAttribRelate?: (ctx: AttribRelateContext) => void;

	/**
	 * Enter a parse tree produced by `ScssParser.identifier`.
	 * @param ctx the parse tree
	 */
	enterIdentifier?: (ctx: IdentifierContext) => void;
	/**
	 * Exit a parse tree produced by `ScssParser.identifier`.
	 * @param ctx the parse tree
	 */
	exitIdentifier?: (ctx: IdentifierContext) => void;

	/**
	 * Enter a parse tree produced by `ScssParser.identifierPart`.
	 * @param ctx the parse tree
	 */
	enterIdentifierPart?: (ctx: IdentifierPartContext) => void;
	/**
	 * Exit a parse tree produced by `ScssParser.identifierPart`.
	 * @param ctx the parse tree
	 */
	exitIdentifierPart?: (ctx: IdentifierPartContext) => void;

	/**
	 * Enter a parse tree produced by `ScssParser.identifierVariableName`.
	 * @param ctx the parse tree
	 */
	enterIdentifierVariableName?: (ctx: IdentifierVariableNameContext) => void;
	/**
	 * Exit a parse tree produced by `ScssParser.identifierVariableName`.
	 * @param ctx the parse tree
	 */
	exitIdentifierVariableName?: (ctx: IdentifierVariableNameContext) => void;

	/**
	 * Enter a parse tree produced by `ScssParser.property`.
	 * @param ctx the parse tree
	 */
	enterProperty?: (ctx: PropertyContext) => void;
	/**
	 * Exit a parse tree produced by `ScssParser.property`.
	 * @param ctx the parse tree
	 */
	exitProperty?: (ctx: PropertyContext) => void;

	/**
	 * Enter a parse tree produced by `ScssParser.values`.
	 * @param ctx the parse tree
	 */
	enterValues?: (ctx: ValuesContext) => void;
	/**
	 * Exit a parse tree produced by `ScssParser.values`.
	 * @param ctx the parse tree
	 */
	exitValues?: (ctx: ValuesContext) => void;

	/**
	 * Enter a parse tree produced by `ScssParser.url`.
	 * @param ctx the parse tree
	 */
	enterUrl?: (ctx: UrlContext) => void;
	/**
	 * Exit a parse tree produced by `ScssParser.url`.
	 * @param ctx the parse tree
	 */
	exitUrl?: (ctx: UrlContext) => void;

	/**
	 * Enter a parse tree produced by `ScssParser.measurement`.
	 * @param ctx the parse tree
	 */
	enterMeasurement?: (ctx: MeasurementContext) => void;
	/**
	 * Exit a parse tree produced by `ScssParser.measurement`.
	 * @param ctx the parse tree
	 */
	exitMeasurement?: (ctx: MeasurementContext) => void;

	/**
	 * Enter a parse tree produced by `ScssParser.functionCall`.
	 * @param ctx the parse tree
	 */
	enterFunctionCall?: (ctx: FunctionCallContext) => void;
	/**
	 * Exit a parse tree produced by `ScssParser.functionCall`.
	 * @param ctx the parse tree
	 */
	exitFunctionCall?: (ctx: FunctionCallContext) => void;
}

