// Generated from src/parser/ScssParser.g4 by ANTLR 4.7.3-SNAPSHOT


import { ParseTreeVisitor } from "antlr4ts/tree/ParseTreeVisitor";

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
 * This interface defines a complete generic visitor for a parse tree produced
 * by `ScssParser`.
 *
 * @param <Result> The return type of the visit operation. Use `void` for
 * operations with no return type.
 */
export interface ScssParserVisitor<Result> extends ParseTreeVisitor<Result> {
	/**
	 * Visit a parse tree produced by `ScssParser.stylesheet`.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	visitStylesheet?: (ctx: StylesheetContext) => Result;

	/**
	 * Visit a parse tree produced by `ScssParser.statement`.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	visitStatement?: (ctx: StatementContext) => Result;

	/**
	 * Visit a parse tree produced by `ScssParser.params`.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	visitParams?: (ctx: ParamsContext) => Result;

	/**
	 * Visit a parse tree produced by `ScssParser.param`.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	visitParam?: (ctx: ParamContext) => Result;

	/**
	 * Visit a parse tree produced by `ScssParser.variableName`.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	visitVariableName?: (ctx: VariableNameContext) => Result;

	/**
	 * Visit a parse tree produced by `ScssParser.paramOptionalValue`.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	visitParamOptionalValue?: (ctx: ParamOptionalValueContext) => Result;

	/**
	 * Visit a parse tree produced by `ScssParser.mixinDeclaration`.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	visitMixinDeclaration?: (ctx: MixinDeclarationContext) => Result;

	/**
	 * Visit a parse tree produced by `ScssParser.includeDeclaration`.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	visitIncludeDeclaration?: (ctx: IncludeDeclarationContext) => Result;

	/**
	 * Visit a parse tree produced by `ScssParser.functionDeclaration`.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	visitFunctionDeclaration?: (ctx: FunctionDeclarationContext) => Result;

	/**
	 * Visit a parse tree produced by `ScssParser.functionBody`.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	visitFunctionBody?: (ctx: FunctionBodyContext) => Result;

	/**
	 * Visit a parse tree produced by `ScssParser.functionReturn`.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	visitFunctionReturn?: (ctx: FunctionReturnContext) => Result;

	/**
	 * Visit a parse tree produced by `ScssParser.functionStatement`.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	visitFunctionStatement?: (ctx: FunctionStatementContext) => Result;

	/**
	 * Visit a parse tree produced by `ScssParser.commandStatement`.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	visitCommandStatement?: (ctx: CommandStatementContext) => Result;

	/**
	 * Visit a parse tree produced by `ScssParser.mathCharacter`.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	visitMathCharacter?: (ctx: MathCharacterContext) => Result;

	/**
	 * Visit a parse tree produced by `ScssParser.mathStatement`.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	visitMathStatement?: (ctx: MathStatementContext) => Result;

	/**
	 * Visit a parse tree produced by `ScssParser.expression`.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	visitExpression?: (ctx: ExpressionContext) => Result;

	/**
	 * Visit a parse tree produced by `ScssParser.ifDeclaration`.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	visitIfDeclaration?: (ctx: IfDeclarationContext) => Result;

	/**
	 * Visit a parse tree produced by `ScssParser.elseIfStatement`.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	visitElseIfStatement?: (ctx: ElseIfStatementContext) => Result;

	/**
	 * Visit a parse tree produced by `ScssParser.elseStatement`.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	visitElseStatement?: (ctx: ElseStatementContext) => Result;

	/**
	 * Visit a parse tree produced by `ScssParser.conditions`.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	visitConditions?: (ctx: ConditionsContext) => Result;

	/**
	 * Visit a parse tree produced by `ScssParser.condition`.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	visitCondition?: (ctx: ConditionContext) => Result;

	/**
	 * Visit a parse tree produced by `ScssParser.variableDeclaration`.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	visitVariableDeclaration?: (ctx: VariableDeclarationContext) => Result;

	/**
	 * Visit a parse tree produced by `ScssParser.forDeclaration`.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	visitForDeclaration?: (ctx: ForDeclarationContext) => Result;

	/**
	 * Visit a parse tree produced by `ScssParser.fromNumber`.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	visitFromNumber?: (ctx: FromNumberContext) => Result;

	/**
	 * Visit a parse tree produced by `ScssParser.throughNumber`.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	visitThroughNumber?: (ctx: ThroughNumberContext) => Result;

	/**
	 * Visit a parse tree produced by `ScssParser.whileDeclaration`.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	visitWhileDeclaration?: (ctx: WhileDeclarationContext) => Result;

	/**
	 * Visit a parse tree produced by `ScssParser.eachDeclaration`.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	visitEachDeclaration?: (ctx: EachDeclarationContext) => Result;

	/**
	 * Visit a parse tree produced by `ScssParser.eachValueList`.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	visitEachValueList?: (ctx: EachValueListContext) => Result;

	/**
	 * Visit a parse tree produced by `ScssParser.identifierListOrMap`.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	visitIdentifierListOrMap?: (ctx: IdentifierListOrMapContext) => Result;

	/**
	 * Visit a parse tree produced by `ScssParser.identifierValue`.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	visitIdentifierValue?: (ctx: IdentifierValueContext) => Result;

	/**
	 * Visit a parse tree produced by `ScssParser.importDeclaration`.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	visitImportDeclaration?: (ctx: ImportDeclarationContext) => Result;

	/**
	 * Visit a parse tree produced by `ScssParser.referenceUrl`.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	visitReferenceUrl?: (ctx: ReferenceUrlContext) => Result;

	/**
	 * Visit a parse tree produced by `ScssParser.mediaTypes`.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	visitMediaTypes?: (ctx: MediaTypesContext) => Result;

	/**
	 * Visit a parse tree produced by `ScssParser.nested`.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	visitNested?: (ctx: NestedContext) => Result;

	/**
	 * Visit a parse tree produced by `ScssParser.nest`.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	visitNest?: (ctx: NestContext) => Result;

	/**
	 * Visit a parse tree produced by `ScssParser.ruleset`.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	visitRuleset?: (ctx: RulesetContext) => Result;

	/**
	 * Visit a parse tree produced by `ScssParser.block`.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	visitBlock?: (ctx: BlockContext) => Result;

	/**
	 * Visit a parse tree produced by `ScssParser.selectors`.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	visitSelectors?: (ctx: SelectorsContext) => Result;

	/**
	 * Visit a parse tree produced by `ScssParser.selector`.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	visitSelector?: (ctx: SelectorContext) => Result;

	/**
	 * Visit a parse tree produced by `ScssParser.selectorPrefix`.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	visitSelectorPrefix?: (ctx: SelectorPrefixContext) => Result;

	/**
	 * Visit a parse tree produced by `ScssParser.element`.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	visitElement?: (ctx: ElementContext) => Result;

	/**
	 * Visit a parse tree produced by `ScssParser.pseudo`.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	visitPseudo?: (ctx: PseudoContext) => Result;

	/**
	 * Visit a parse tree produced by `ScssParser.attrib`.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	visitAttrib?: (ctx: AttribContext) => Result;

	/**
	 * Visit a parse tree produced by `ScssParser.attribRelate`.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	visitAttribRelate?: (ctx: AttribRelateContext) => Result;

	/**
	 * Visit a parse tree produced by `ScssParser.identifier`.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	visitIdentifier?: (ctx: IdentifierContext) => Result;

	/**
	 * Visit a parse tree produced by `ScssParser.identifierPart`.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	visitIdentifierPart?: (ctx: IdentifierPartContext) => Result;

	/**
	 * Visit a parse tree produced by `ScssParser.identifierVariableName`.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	visitIdentifierVariableName?: (ctx: IdentifierVariableNameContext) => Result;

	/**
	 * Visit a parse tree produced by `ScssParser.property`.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	visitProperty?: (ctx: PropertyContext) => Result;

	/**
	 * Visit a parse tree produced by `ScssParser.values`.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	visitValues?: (ctx: ValuesContext) => Result;

	/**
	 * Visit a parse tree produced by `ScssParser.url`.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	visitUrl?: (ctx: UrlContext) => Result;

	/**
	 * Visit a parse tree produced by `ScssParser.measurement`.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	visitMeasurement?: (ctx: MeasurementContext) => Result;

	/**
	 * Visit a parse tree produced by `ScssParser.functionCall`.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	visitFunctionCall?: (ctx: FunctionCallContext) => Result;
}

