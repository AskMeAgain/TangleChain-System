using System;
using System.Collections.Generic;
using System.Text;
using StrainLanguage.Classes;

namespace StrainLanguage.NodeClasses
{
    public class ExpressionNode : Node
    {
        public string _expression { get; protected set; }

        public ExpressionNode(string exp)
        {
            _expression = exp;
            Nodes = new List<Node>() { ConvertExpressionToNode(exp) };
        }

        public Node ConvertExpressionToNode(string expression)
        {

            var helper = new ExpressionHelper(expression);

            if (helper.Length == 1)
            {

                //we need to see if the value is a variable int or string
                string type = string.Empty;
                string value = string.Empty;

                //its a string
                if (helper.Contains("\""))
                {
                    type = "string";
                    value = helper[0];
                    return new ValueNode(value, type);
                }

                var flag = int.TryParse(helper[0], out int result);
                var flag2 = int.TryParse(helper[0], out int result2);

                //its an int
                if (flag)
                {
                    type = "int";
                    value = helper[0].ToString();
                    return new ValueNode(value, type);
                }

                //its a long
                if (flag2)
                {
                    type = "long";
                    value = helper[0].ToString();
                    return new ValueNode(value, type);
                }

                //it now means its a preused variable
                return new VariableNode(helper[0]);

            }

            throw new NotImplementedException("still not really implemented!");
        }
    }
}
