//Willem DeJong
using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Text.RegularExpressions;
using System.Data;
using System.Data.Common;

namespace _2bn2b
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        private void submit_Click(object sender, RoutedEventArgs e)
        {
            table.clear();
            RootNode tree = new RootNode(null, null);
            try
            {
                tree = NodeWork.build(expBox.Text);
                expBox.BorderBrush = new SolidColorBrush(Colors.Black);
                numVBox.Text = Convert.ToString(tree.variables.Count);
                string vars = "";
                foreach (string var in tree.variables)
                {
                   
                    table.addColumn(var);
                    vars = vars + var + ", ";
                }

                vars = vars.Substring(0, vars.Length - 2);
                LV.Text = vars;

            }
            catch
            {
                expBox.BorderBrush = new SolidColorBrush(Colors.Red);
                return;
            }
            for (int i = 0; i < Math.Pow(2, tree.variables.Count); i++)
            {
                table.addRow();
            }
            foreach (string var in tree.variables)
            {
                int i = table.findColumn(var);
                bool flip = false;
                for (int j = 1; j <= Math.Pow(2, tree.variables.Count); j++)
                {
                    if (flip)
                        table.setText(i,j,"T");
                    else
                        table.setText(i, j, "F");
                    if (j % (Math.Pow(2, tree.variables.Count) / Math.Pow(2, i+1)) == 0)
                        flip = !flip;
                }
            }
            List<string> cols = tree.specialToString();
            foreach(string var in cols)
            {
                table.addColumn(var);
            }
            for (int j = 1; j < table.count(); j++)
            {
                foreach (string var in tree.variables)
                {
                    int i = table.findColumn(var);
                    if (table.getText(i, j) == "T")
                    {
                        tree.setValValue(var, true);
                    }
                    else
                    {
                        tree.setValValue(var, false);
                    }
                }
                List<bool> vals=tree.specialEval();
                for (int ind = 0; ind < cols.Count; ind++)
                {
                    int i = table.findColumn(cols[ind]);
                    if (vals[ind])
                        table.setText(i, j, "T");
                    else
                        table.setText(i, j, "F");
                }
            }

        }

    }
    public class tableData : ScrollViewer
    {
        private int fontSize=12;
        private Grid tableGrid;
        private List<List<TextBox>> rows;
        public tableData()
        {
            rows = new List<List<TextBox>>();
            rows.Add(new List<TextBox>());
            tableGrid = new Grid();
            this.Content = tableGrid;
        }
        private TextBox createBox(string tex)
        {
            TextBox temp = new TextBox();
            temp.IsReadOnly = true;
            temp.Focusable = false;
            temp.BorderBrush = new SolidColorBrush(Colors.Black);
            temp.HorizontalAlignment = HorizontalAlignment.Left;
            temp.VerticalAlignment = VerticalAlignment.Top;
            temp.TextAlignment = TextAlignment.Center;
            temp.Text="  "+tex+"  ";
            temp.FontSize=fontSize;
            temp.Width= fontSize* temp.Text.Length * 0.65;
            temp.Height = fontSize*1.5;
            return temp;
        }
        private TextBox createBox(int i,int j)
        {
            TextBox temp = new TextBox();
            temp.IsReadOnly = true;
            temp.Focusable = false;
            temp.BorderBrush = new SolidColorBrush(Colors.Black);
            temp.HorizontalAlignment = HorizontalAlignment.Left;
            temp.VerticalAlignment = VerticalAlignment.Top;
            temp.TextAlignment = TextAlignment.Center;
            temp.Text="";
            temp.FontSize=fontSize;
            temp.Width= rows[0][i].Width;
            temp.Height = rows[0][i].Height;
            double left=0;
            if(i>0)
                left=rows[j][i-1].Margin.Left+rows[j][i-1].Width;
            double top=0;
            if(j>0)
                top=rows[j-1][i].Margin.Top+rows[j-1][i].Height;
            temp.Margin=new Thickness(left,top,0,0);
            return temp;
        }
        public int findColumn(string col)
        {
            int cnt=0;
            while(cnt<rows[0].Count)
            {
                if(rows[0][cnt].Text==("  "+col+"  "))
                {
                    return cnt;
                }
                cnt++;
            }
            return -1;
        }
        public int addColumn(string col)
        {
            if (findColumn(col) != -1)
                return -1;
            TextBox column = createBox(col);
            if (rows[0].Count > 0)
            {
                column.Margin = new Thickness(rows[0][rows[0].Count - 1].Width + rows[0][rows[0].Count - 1].Margin.Left, 0, 0, 0);
            }
            else
            {
                column.Margin = new Thickness(0, 0, 0, 0);
            }
            tableGrid.Children.Add(column);
            rows[0].Add(column);
            int cnt = 1;
            while (cnt < rows.Count)
            {
                TextBox cell = createBox(rows[cnt].Count, cnt);
                rows[cnt].Add(cell);
                tableGrid.Children.Add(cell);
                cnt++;
            }
            return rows[0].Count - 1;
        }
        public int addRow()
        {
            rows.Add(new List<TextBox>());
            int cnt = 0;
            int j = rows.Count - 1;
            while (cnt < rows[0].Count)
            {
                TextBox cell = createBox(cnt, j);
                rows[j].Add(cell);
                tableGrid.Children.Add(cell);
                cnt++;
            }
            return j;
        }
        public void setText(int i, int j, string txt)
        {
            rows[j][i].Text = txt;
        }
        public string getText(int i, int j)
        {
            return rows[j][i].Text;
        }
        public int count()
        {
            return rows.Count;
        }
        public int count(int j)
        {
            return rows[j].Count;
        }
        public void clear()
        {
            tableGrid.Children.Clear();
            rows.Clear();
            rows.Add(new List<TextBox>());
        }
    }
    public class NumBox : TextBox
    {
        public double min{get;set;}
        public double max { get; set; }
        private string current = "2";
        public NumBox()
        {
            min = 0;
            max = 1000; 
            Text = "2";
            TextChanged += NumTextChanged;
        }
        private void NumTextChanged(object sender, TextChangedEventArgs e)
        {
            int curcind = CaretIndex;
            if (Regex.IsMatch(Text,@"^\-?[0-9]+$"))
            {
                if (Convert.ToDouble(Text) > max)
                {
                    Text = Convert.ToString(max);
                    current = Text;
                    CaretIndex = Text.Length;
                }
                else if (Convert.ToDouble(Text) < min)
                {
                    Text = Convert.ToString(min);
                    current = Text;
                    CaretIndex = Text.Length;
                }
                else
                    current = Text;
            }
            else
            {
                Text = current;
                CaretIndex = curcind - 1;
            }
        }
    }
    //this class contains the functions used to build a logic tree
    public class NodeWork
    {
        static Stack<string> operatorStack;
        static Stack<LogNode> operandStack;
        //this function returns the precedence of  the operator
        private static int getPrecedence(string op)
        {
            switch (op)//modify here to add new operator
            {
                case "&":
                case "|":
                case "->":
                case "=":
                case "><":
                    return 2;
                //not sure if having this as a higher precedence really does anything
                case "!":
                    return 3;

                default:
                    return 1;
            }
        }
        //this function builds and returns the logic tree.
        public static RootNode build(string exp)
        {
            //opn is a hashSet that will contain each unique variable in the logic tree, this is to be stored in the rootNode inorder for the user of the tree to have access to the set of variable since a single variable can appear in the tree multiple times
            HashSet<string> opn = new HashSet<string>();
            exp = exp.Replace(" ", "");//removes all spaces. should modify to account for all white space
            operatorStack = new Stack<string>();//stack for operators while using the stack method for building a operation tree from infix
            operandStack = new Stack<LogNode>();//stack for operands(nodes/subtrees) while using the stack method for building a operation tree from infix
            MatchCollection mexp = Regex.Matches(exp, @"[A-Za-z0-9]+|&|\||!|\(|\)|><|=|->");//this will create an iteratable object that each operand and operator has been seperated into it's own element, this allows for multi-character operand/operators. assumes order is not lost.
            string[] expr = new string[mexp.Count];//the values in the match collection are to be put into this array
            int ind = 0;//an integer to be used as an index for the above array while iterating through mexp.
            foreach (Match match in mexp)//puts the value of each match in mexp into the array
            {
                expr[ind] = match.Value;
                ind++;
            }
            foreach (string token in expr)//this loop goes through each token (operator/operand) and handles accordingly
            {
                switch (token)
                {
                    case "&"://modify here to add new operator
                    case "|":
                    case "->":
                    case "=":
                    case "><":
                    case "!"://if the token is an operator
                        while ((operatorStack.Count != 0) && (getPrecedence(token) <= getPrecedence(operatorStack.Peek())))//while the operator stack is not empty and the precedence of the token is less than or equal to that on the top of the stack.
                        {
                            createOpNode(operatorStack.Pop());//pop the top operand and call createOpNode with it.
                        }
                        operatorStack.Push(token);//pushes the token onto the stack
                        continue;//go to the next token
                    case ")"://if the token is a right parenthesis 
                        while ((operatorStack.Peek() != "(")) //loops through the operator stack until it hits a left parenthesis
                        {
                            if (operatorStack.Count == 0)//if the stack is empty and we haven't fount the right parenthesis then the given string for the argument was in an invalid format.
                            {
                                throw (new FormatException());
                            }
                            createOpNode(operatorStack.Pop());//removes the operator and calls createOpNode with it
                        }
                        createOpNode(operatorStack.Pop());//pops off the right parenthesis and calls createOpNode with it.
                        continue;//go to the next token
                    case "("://if the token is a left parenthesis push it to the operator stack
                        operatorStack.Push(token);
                        continue;//go to the next token
                    default://else its operands
                        opn.Add(token);//add the operand to opn
                        ValNode newNode = new ValNode(true, token);//create a vlue node
                        operandStack.Push(newNode);//push the node onto the operand stack
                        continue;//go to the next token
                }
            }
            while (operatorStack.Count != 0)//while there are still operands in the stack
            {
                createOpNode(operatorStack.Pop());//pop the top operator and call createOpNode with it.
            }
            if (operandStack.Count != 1)//if there isn't a single logic subtree left by this point the string given as an argument was invalid
            {
                throw (new FormatException());
            }
            RootNode root = new RootNode(operandStack.Pop(), opn);//builds the complete logic tree from the last logic subtree
            return root;
            //return operandStack.Peek();
        }
        private static void createOpNode(string op)//creates the opropriate logic node for the operator using the opperands on top of the operand stack
        {
            LogNode a;
            switch (op)
            {//modify here to add new operator
                case "&":
                    a = new AndNode(operandStack.Pop(), operandStack.Pop());
                    break;
                case "|":
                    a = new OrNode(operandStack.Pop(), operandStack.Pop());
                    break;
                case "->":
                    a = new IfNode(operandStack.Pop(), operandStack.Pop());
                    break;
                case "=":
                    a = new EqNode(operandStack.Pop(), operandStack.Pop());
                    break;
                case "><":
                    a = new XOrNode(operandStack.Pop(), operandStack.Pop());
                    break;
                case "!":
                    a = new NotNode(operandStack.Pop());
                    break;
                case "(":
                    a = new ParNode(operandStack.Pop());
                    break;
                default://not an operator
                    throw (new FormatException());
            }
            operandStack.Push(a);
        }
    }
    public interface LogNode//interface of a logical node
    {
        bool eval(); //handles the logical operation on it's child node's eval or if a value node returns the bool value following an in order traversal.
        string toString();//through in order traversal it recreates the string used to make the tree
        void setValValue(string valName, bool valValue);//taverses to each ValNode(leaf node) and compares 'valName' to the ValNode's valueName, and if they match the value is set to valValue
        List<string> specialToString();//through post order traversal it makes a list of the toString()s of each subtree and itself
        List<bool> specialEval();//through post order traversal it makes a list of the eval()s of each subtree and itself
        HashSet<string> getValNames();
    }
    public class RootNode : LogNode// root node, contains handy info on the rest of the tree like a set of each unique variable intended to never be in a subtree.
    {
        public LogNode next { get; set; }//the single branch to the next node
        public HashSet<string> variables;//the set of each variable
        public RootNode(LogNode nx, HashSet<string> varis)//constructor. never intended to have a RootNode with out a child hence no defualt constructor also should have a path to a ValNode to be a proper and useful logcal tree.
        {
            variables = varis;
            next = nx;
        }
        public bool eval()//since the root contributes nothing to the logical operations it just calls the eval() of its child 'next' and returns the result
        {
            return next.eval();
        }
        public string toString()//since the root contributes nothing to the string it just calls the toString() of its child 'next' and returns the result
        {
            return next.toString();
        }
        public void setValValue(string valName, bool valValue)//since the root is not a ValNode (leaf node) it does not have a value so it calls setValValue of its Child
        {
            next.setValValue(valName, valValue);
        }
        public List<string> specialToString()//since the root contributes nothing to the string it just calls the specialToString() of its child 'next' and returns the result
        {
            return next.specialToString();
        }
        public List<bool> specialEval()//since the root contributes nothing to the logical operations it just calls the specialEval() of its child 'next' and returns the result
        {
            return next.specialEval();
        }
        public HashSet<string> getValNames()
        {
            return variables;
        }
    }
    public class ValNode : LogNode//intended as the leaf node. all leaf nodes should be ValNodes for the logic to work properly
    {
        public bool value { get; set; }//this is the true or false value of this node
        public string valueName { get; set; }//the variable name corrisponding to this node
        public ValNode(bool val, string name)//constructor. no defualt because it should have a meaningful name and value
        {
            value = val;
            valueName = name;
        }
        public bool eval()//eval returns the value
        {
            return value;
        }
        public string toString()//toString returns the name
        {
            return valueName;
        }
        public void setValValue(string valName, bool valValue)//compares 'valName' to the ValNode's valueName, and if they match the value is set to valValue
        {
            if (valName == valueName)
                value = valValue;
        }
        public List<string> specialToString()//creates an empty list and returns it, the intended purpose of this function is to get each step/subtree as a seperate string in the list. so the individual name of the values is unwanted.
        {
            List<string> temp = new List<string>();
            //temp.Add(valueName);
            return temp;
        }
        public List<bool> specialEval()//creates an empty list and returns it, the intended purpose of this function is to get each resulting value for each step/subtree in the list. so the individual values is unwanted.
        {
            List<bool> temp = new List<bool>();
            //temp.Add(value);
            return temp;
        }
        public HashSet<string> getValNames()
        {
            HashSet<string> names=new HashSet<string>();
            names.Add(valueName);
            return names;
        }
    }
    public class NotNode : LogNode//Node for the 'not' operator
    {
        public LogNode rightNode { get; set; }
        public NotNode(LogNode R)//cannot exist without a value to operate on
        {
            rightNode = R;
        }
        public bool eval()//returns the negation of the child's eval().
        {
            return !rightNode.eval();
        }
        public string toString() //returns the concatination of "! " and the string of the subtree in rightNode
        {
            return "! " + rightNode.toString();
        }
        public void setValValue(string valName, bool valValue) //since the not node is not a ValNode (leaf node) it does not have a value so it calls setValValue of its Child
        {
            rightNode.setValValue(valName, valValue);
        }
        public List<string> specialToString()
        {
            List<string> temp = rightNode.specialToString();//gets the resulting list from the specialToString of the child.
            temp.Add(this.toString());//adds this node's toString to the end of the list
            return temp;
        }
        public List<bool> specialEval()
        {
            List<bool> temp = rightNode.specialEval();//gets the resulting list from the specialEval of the child.
            temp.Add(this.eval());//adds this node's eval to the end of the list
            return temp;
        }
        public HashSet<string> getValNames()
        {
            return rightNode.getValNames();
        }
    }
    public class ParNode : LogNode//node to represent parenthesis. allows for keeping the parenthesis when recreating the string
    {
        public LogNode rightNode { get; set; }
        public ParNode(LogNode R)
        {
            rightNode = R;
        }
        public bool eval()//since the parenthesis contributes nothing to the logical operations it just calls the eval() of its child 'rightNode' and returns the result
        {
            return rightNode.eval();
        }
        public string toString()//returns the string of the child's toString surrounded by ()
        {
            return "( " + rightNode.toString() + " )";
        }
        public void setValValue(string valName, bool valValue)//since the root is not a ValNode (leaf node) it does not have a value so it calls setValValue of its Child
        {
            rightNode.setValValue(valName, valValue);
        }
        public List<string> specialToString()//since the root contributes nothing to the string it just calls the specialToString() of its child 'rightNode' and returns the result
        {
            List<string> temp = rightNode.specialToString();
            //temp.Add(this.toString());
            return temp;
        }
        public List<bool> specialEval()//since the root contributes nothing logically in this format it just calls the specialEval() of its child 'rightNode' and returns the result
        {
            List<bool> temp = rightNode.specialEval();
            //temp.Add(this.toString());
            return temp;
        }
        public HashSet<string> getValNames()
        {
            return rightNode.getValNames();
        }
    }
    public class AndNode : LogNode//Node for the 'and' operator
    {
        public LogNode leftNode { get; set; }
        public LogNode rightNode { get; set; }
        public AndNode(LogNode R, LogNode L)//constructor. must have a left and right child.
        {
            leftNode = L;
            rightNode = R;
        }
        public bool eval()//does the 'and' operation between the result of the left and right subtree
        {
            return leftNode.eval() && rightNode.eval();
        }
        public string toString()//returns the string created by the concatination of the tostring of the left child, " & ", and the toString of the right child
        {
            return leftNode.toString() + " & " + rightNode.toString();
        }
        public void setValValue(string valName, bool valValue)//since the root is not a ValNode (leaf node) it does not have a value so it calls setValValue of its Children
        {
            leftNode.setValValue(valName, valValue);
            rightNode.setValValue(valName, valValue);
        }
        public List<string> specialToString()
        {
            List<string> temp1 = leftNode.specialToString();//gets the list from the left subtree
            List<string> temp2 = rightNode.specialToString();//gets the list from the right subtree
            List<string> temp=temp1.Concat(temp2).ToList();//concatinate the two lists
            temp.Add(this.toString());//add this tree's toString
            return temp;
        }
        public List<bool> specialEval()
        {
            List<bool> temp1 = leftNode.specialEval();//gets the list from the left subtree
            List<bool> temp2 = rightNode.specialEval();//gets the list from the right subtree
            List<bool> temp = temp1.Concat(temp2).ToList();//concatinate the two lists
            temp.Add(this.eval());//add this tree's toString
            return temp;
        }
        public HashSet<string> getValNames()
        {
            HashSet<string> names= leftNode.getValNames();
            foreach (string name in rightNode.getValNames())
            {
                names.Add(name);
            }
            return names;
        }
    }
    public class OrNode : LogNode//Node for the 'or' operator
    {
        public LogNode leftNode { get; set; }
        public LogNode rightNode { get; set; }
        public OrNode(LogNode R, LogNode L)//constructor. must have a left and right child.
        {
            leftNode = L;
            rightNode = R;
        }
        public bool eval()//does the 'or' operation between the result of the left and right subtree
        {
            return leftNode.eval() || rightNode.eval();
        }
        public string toString()//returns the string created by the concatination of the tostring of the left child, " | ", and the toString of the right child
        {
            return leftNode.toString() + " | " + rightNode.toString();
        }
        public void setValValue(string valName, bool valValue)//since the root is not a ValNode (leaf node) it does not have a value so it calls setValValue of its Children
        {
            leftNode.setValValue(valName, valValue);
            rightNode.setValValue(valName, valValue);
        }
        public List<string> specialToString()
        {
            List<string> temp1 = leftNode.specialToString();//gets the list from the left subtree
            List<string> temp2 = rightNode.specialToString();//gets the list from the right subtree
            List<string> temp = temp1.Concat(temp2).ToList();//concatinate the two lists
            temp.Add(this.toString());//add this tree's toString
            return temp;
        }
        public List<bool> specialEval()
        {
            List<bool> temp1 = leftNode.specialEval();//gets the list from the left subtree
            List<bool> temp2 = rightNode.specialEval();//gets the list from the right subtree
            List<bool> temp = temp1.Concat(temp2).ToList();//concatinate the two lists
            temp.Add(this.eval());//add this tree's toString
            return temp;
        }
        public HashSet<string> getValNames()
        {
            HashSet<string> names = leftNode.getValNames();
            foreach (string name in rightNode.getValNames())
            {
                names.Add(name);
            }
            return names;
        }
    }
    public class XOrNode : LogNode//Node for the 'exclusive or' operator
    {
        public LogNode leftNode { get; set; }
        public LogNode rightNode { get; set; }
        public XOrNode(LogNode R, LogNode L)//constructor. must have a left and right child.
        {
            leftNode = L;
            rightNode = R;
        }
        public bool eval()//does the 'exclusive or' operation between the result of the left and right subtree using a locigal equivilant using just and,! and or.
        {
            return (leftNode.eval() || rightNode.eval()) && !(leftNode.eval() && rightNode.eval());
        }
        public string toString()//returns the string created by the concatination of the tostring of the left child, " >< ", and the toString of the right child
        {
            return leftNode.toString() + " >< " + rightNode.toString();
        }
        public void setValValue(string valName, bool valValue)//since the root is not a ValNode (leaf node) it does not have a value so it calls setValValue of its Children
        {
            leftNode.setValValue(valName, valValue);
            rightNode.setValValue(valName, valValue);
        }
        public List<string> specialToString()
        {
            List<string> temp1 = leftNode.specialToString();//gets the list from the left subtree
            List<string> temp2 = rightNode.specialToString();//gets the list from the right subtree
            List<string> temp = temp1.Concat(temp2).ToList();//concatinate the two lists
            temp.Add(this.toString());//add this tree's toString
            return temp;
        }
        public List<bool> specialEval()
        {
            List<bool> temp1 = leftNode.specialEval();//gets the list from the left subtree
            List<bool> temp2 = rightNode.specialEval();//gets the list from the right subtree
            List<bool> temp = temp1.Concat(temp2).ToList();//concatinate the two lists
            temp.Add(this.eval());//add this tree's toString
            return temp;
        }
        public HashSet<string> getValNames()
        {
            HashSet<string> names = leftNode.getValNames();
            names.Concat(rightNode.getValNames());
            return names;
        }
    }
    public class EqNode : LogNode//Node for the 'equivilance' or 'if and only if' operator
    {
        public LogNode leftNode { get; set; }
        public LogNode rightNode { get; set; }
        public EqNode(LogNode R, LogNode L)//constructor. must have a left and right child.
        {
            leftNode = L;
            rightNode = R;
        }
        public bool eval()//does the 'if and only if' operation between the result of the left and right subtree using a locigal equivilant using just and,! and or.
        {
            return (leftNode.eval() && rightNode.eval()) || (!leftNode.eval() && !rightNode.eval());
        }
        public string toString()//returns the string created by the concatination of the tostring of the left child, " = ", and the toString of the right child
        {
            return leftNode.toString() + " = " + rightNode.toString();
        }
        public void setValValue(string valName, bool valValue)//since the root is not a ValNode (leaf node) it does not have a value so it calls setValValue of its Children
        {
            leftNode.setValValue(valName, valValue);
            rightNode.setValValue(valName, valValue);
        }
        public List<string> specialToString()
        {
            List<string> temp1 = leftNode.specialToString();//gets the list from the left subtree
            List<string> temp2 = rightNode.specialToString();//gets the list from the right subtree
            List<string> temp = temp1.Concat(temp2).ToList();//concatinate the two lists
            temp.Add(this.toString());//add this tree's toString
            return temp;
        }
        public List<bool> specialEval()
        {
            List<bool> temp1 = leftNode.specialEval();//gets the list from the left subtree
            List<bool> temp2 = rightNode.specialEval();//gets the list from the right subtree
            List<bool> temp = temp1.Concat(temp2).ToList();//concatinate the two lists
            temp.Add(this.eval());//add this tree's toString
            return temp;
        }
        public HashSet<string> getValNames()
        {
            HashSet<string> names = leftNode.getValNames();
            names.Concat(rightNode.getValNames());
            return names;
        }
    }
    public class IfNode : LogNode//Node for the 'if' operator
    {
        public LogNode leftNode { get; set; }
        public LogNode rightNode { get; set; }
        public IfNode(LogNode R, LogNode L)//constructor. must have a left and right child.
        {
            leftNode = L;
            rightNode = R;
        }
        public bool eval()//does the 'if' operation between the result of the left and right subtree using a locigal equivilant using just and,! and or.
        {
            return !leftNode.eval() || rightNode.eval();
        }
        public string toString()//returns the string created by the concatination of the tostring of the left child, " -> ", and the toString of the right child
        {
            return leftNode.toString() + " -> " + rightNode.toString();
        }
        public void setValValue(string valName, bool valValue)//since the root is not a ValNode (leaf node) it does not have a value so it calls setValValue of its Children
        {
            leftNode.setValValue(valName, valValue);
            rightNode.setValValue(valName, valValue);
        }
        public List<string> specialToString()
        {
            List<string> temp1 = leftNode.specialToString();//gets the list from the left subtree
            List<string> temp2 = rightNode.specialToString();//gets the list from the right subtree
            List<string> temp = temp1.Concat(temp2).ToList();//concatinate the two lists
            temp.Add(this.toString());//add this tree's toString
            return temp;
        }
        public List<bool> specialEval()
        {
            List<bool> temp1 = leftNode.specialEval();//gets the list from the left subtree
            List<bool> temp2 = rightNode.specialEval();//gets the list from the right subtree
            List<bool> temp = temp1.Concat(temp2).ToList();//concatinate the two lists
            temp.Add(this.eval());//add this tree's toString
            return temp;
        }
        public HashSet<string> getValNames()
        {
            HashSet<string> names = leftNode.getValNames();
            names.Concat(rightNode.getValNames());
            return names;
        }
    }
}