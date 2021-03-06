' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports Microsoft.CodeAnalysis.Editor.SignatureHelp
Imports Microsoft.CodeAnalysis.Text
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax

Namespace Microsoft.CodeAnalysis.Editor.VisualBasic.SignatureHelp

    Friend Module SignatureHelpUtilities
        Private ReadOnly GetArgumentListOpenToken As Func(Of ArgumentListSyntax, SyntaxToken) = Function(list) list.OpenParenToken
        Private ReadOnly GetTypeArgumentListOpenToken As Func(Of TypeArgumentListSyntax, SyntaxToken) = Function(list) list.OpenParenToken

        Private ReadOnly GetArgumentListCloseToken As Func(Of ArgumentListSyntax, SyntaxToken) =
            Function(list)
                ' In the case where the user has typed "Foo(bar:" then the parser doesn't consider
                ' the colon part of the signature.  However, we want to as it is clearly part of a
                ' named parameter they are in the middle of typing.  So consume it in that case.
                If list.CloseParenToken.IsMissing Then
                    Dim nextToken = list.GetLastToken().GetNextToken()
                    Dim nextNextToken = nextToken.GetNextToken()
                    If nextToken.Kind = SyntaxKind.ColonToken AndAlso nextNextToken.Kind <> SyntaxKind.None Then
                        Return nextNextToken
                    End If
                End If

                Return list.CloseParenToken()
            End Function

        Private ReadOnly GetTypeArgumentListCloseToken As Func(Of TypeArgumentListSyntax, SyntaxToken) = Function(list) list.CloseParenToken

        Private ReadOnly GetArgumentListArgumentsWithSeparators As Func(Of ArgumentListSyntax, IEnumerable(Of SyntaxNodeOrToken)) = Function(list) list.Arguments.GetWithSeparators().AsEnumerable().[Select](Function(t) CType(t, SyntaxNodeOrToken))
        Private ReadOnly GetTypeArgumentListArgumentsWithSeparators As Func(Of TypeArgumentListSyntax, IEnumerable(Of SyntaxNodeOrToken)) = Function(list) list.Arguments.GetWithSeparators().AsEnumerable().[Select](Function(t) CType(t, SyntaxNodeOrToken))

        Private ReadOnly GetArgumentListNames As Func(Of ArgumentListSyntax, IEnumerable(Of String)) =
            Function(list) list.Arguments.Select(Function(a)
                                                     Dim simpleArgument = TryCast(a, SimpleArgumentSyntax)
                                                     Dim value = If(simpleArgument IsNot Nothing AndAlso simpleArgument.NameColonEquals IsNot Nothing,
                                                                    simpleArgument.NameColonEquals.Name.Identifier.ValueText,
                                                                    Nothing)
                                                     Return If(String.IsNullOrEmpty(value), Nothing, value)
                                                 End Function)

        Private ReadOnly GetTypeArgumentListNames As Func(Of TypeArgumentListSyntax, IEnumerable(Of String)) = Function(list) list.Arguments.Select(Function(a) DirectCast(Nothing, String))

        Friend Function GetSignatureHelpSpan(argumentList As ArgumentListSyntax) As TextSpan
            Return CommonSignatureHelpUtilities.GetSignatureHelpSpan(argumentList, GetArgumentListCloseToken)
        End Function

        Friend Function GetSignatureHelpSpan(argumentList As ArgumentListSyntax, start As Integer) As TextSpan
            Return CommonSignatureHelpUtilities.GetSignatureHelpSpan(argumentList, start, GetArgumentListCloseToken)
        End Function

        Friend Function GetSignatureHelpSpan(argumentList As TypeArgumentListSyntax) As TextSpan
            Return CommonSignatureHelpUtilities.GetSignatureHelpSpan(argumentList, GetTypeArgumentListCloseToken)
        End Function

        Friend Function GetSignatureHelpState(argumentList As ArgumentListSyntax, position As Integer) As SignatureHelpState

            Return CommonSignatureHelpUtilities.GetSignatureHelpState(
                argumentList,
                position,
                GetArgumentListOpenToken,
                GetArgumentListCloseToken,
                GetArgumentListArgumentsWithSeparators,
                GetArgumentListNames)
        End Function

        Friend Function GetSignatureHelpState(typeArgumentList As TypeArgumentListSyntax, position As Integer) As SignatureHelpState
            Return CommonSignatureHelpUtilities.GetSignatureHelpState(
                typeArgumentList,
                position,
                GetTypeArgumentListOpenToken,
                GetTypeArgumentListCloseToken,
                GetTypeArgumentListArgumentsWithSeparators,
                GetTypeArgumentListNames)
        End Function

    End Module
End Namespace

