using System;
using System.Collections.Generic;
class ExpressionStmt : Stmt
{
    Expression expression;
    public ExpressionStmt(Expression exp, CodeLocation location) : base(location)
    {
        this.expression = exp;
    }
    public override Scope AssociatedScope {get; set;}
    public override bool CheckSemantic(Context context, Scope scope, List<CompilingError> errors)
    {
        this.AssociatedScope = scope;
        bool valid = expression.CheckSemantic(context, AssociatedScope, errors);
        if(expression.Type != ExpressionType.Function)
        {
            errors.Add(new CompilingError(Location, ErrorCode.Invalid, "Expresion invalida, solo se permiten funciones y cuerpos de expresiones"));
            return false;
        }
        return valid;
    }
    public override void Interprete()
    {
        expression.Evaluate();
    }
    public override string ToString()
    {
        return expression.ToString();
    }
}