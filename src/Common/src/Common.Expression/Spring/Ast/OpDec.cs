// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information.

using System;

namespace Steeltoe.Common.Expression.Internal.Spring.Ast;

public class OpDec : Operator
{
    private readonly bool _postfix;  // false means prefix

    public OpDec(int startPos, int endPos, bool postfix, params SpelNode[] operands)
        : base("--", startPos, endPos, operands)
    {
        _postfix = postfix;
        if (operands == null || operands.Length == 0)
        {
            throw new InvalidOperationException("Operands can not be empty");
        }
    }

    public override ITypedValue GetValueInternal(ExpressionState state)
    {
        var operand = LeftOperand;

        // The operand is going to be read and then assigned to, we don't want to evaluate it twice.
        var lvalue = operand.GetValueRef(state);

        var operandTypedValue = lvalue.GetValue();
        var operandValue = operandTypedValue.Value;
        var returnValue = operandTypedValue;
        ITypedValue newValue = null;

        if (IsNumber(operandValue))
        {
            newValue = operandValue switch
            {
                decimal val => new TypedValue(val - 1M, operandTypedValue.TypeDescriptor),
                double val => new TypedValue(val - 1.0d, operandTypedValue.TypeDescriptor),
                float val => new TypedValue(val - 1.0f, operandTypedValue.TypeDescriptor),
                long val => new TypedValue(val - 1L, operandTypedValue.TypeDescriptor),
                int val => new TypedValue(val - 1, operandTypedValue.TypeDescriptor),
                short val => new TypedValue((short)(val - 1), operandTypedValue.TypeDescriptor),
                byte val => new TypedValue((byte)(val - 1), operandTypedValue.TypeDescriptor),
                ulong val => new TypedValue(val - 1L, operandTypedValue.TypeDescriptor),
                uint val => new TypedValue(val - 1, operandTypedValue.TypeDescriptor),
                ushort val => new TypedValue((ushort)(val - 1), operandTypedValue.TypeDescriptor),
                sbyte val => new TypedValue((sbyte)(val - 1), operandTypedValue.TypeDescriptor),
                _ => null
            };
        }

        if (newValue == null)
        {
            try
            {
                newValue = state.Operate(Operation.Subtract, returnValue.Value, 1);
            }
            catch (SpelEvaluationException ex)
            {
                if (Equals(ex.MessageCode, SpelMessage.OperatorNotSupportedBetweenTypes))
                {
                    // This means the operand is not decrementable
                    throw new SpelEvaluationException(operand.StartPosition, SpelMessage.OperandNotDecrementable, operand.ToStringAst());
                }
                else
                {
                    throw;
                }
            }
        }

        // set the new value
        try
        {
            lvalue.SetValue(newValue.Value);
        }
        catch (SpelEvaluationException see)
        {
            // if unable to set the value the operand is not writable (e.g. 1-- )
            if (Equals(see.MessageCode, SpelMessage.SetValueNotSupported))
            {
                throw new SpelEvaluationException(operand.StartPosition, SpelMessage.OperandNotDecrementable);
            }
            else
            {
                throw;
            }
        }

        if (!_postfix)
        {
            // the return value is the new value, not the original value
            returnValue = newValue;
        }

        return returnValue;
    }

    public override string ToStringAst()
    {
        return $"{LeftOperand.ToStringAst()}--";
    }

    public override SpelNode RightOperand => throw new InvalidOperationException("No right operand");
}
