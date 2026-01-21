using HtmlAgilityPack;

namespace Calcpad.WebApi.Utils.Calcpad
{
    /// <summary>
    /// result of processing a conditional block
    /// </summary>
    class ConditionalBlockResult
    {
        public HtmlNode WrapperDiv { get; set; } = null!;
        public List<HtmlNode> NodesToRemove { get; set; } = new();
    }

    public class ConditionBlockWrapper(HtmlDocument doc)
    {
        /// <summary>
        /// process conditional blocks and wrap them in divs with v-if/v-else-if/v-else attributes
        /// </summary>
        /// <param name="doc"></param>
        public void ProcessConditionalBlocks()
        {
            var allNodes = doc.DocumentNode.ChildNodes.ToList();
            var i = 0;

            while (i < allNodes.Count)
            {
                var node = allNodes[i];

                // check if this is an #if statement
                if (IsConditionalNode(node, out var condType, out var condition))
                {
                    if (condType == "if")
                    {
                        // find the matching #end if
                        var blockResult = ProcessIfBlock(allNodes, i);
                        if (blockResult != null)
                        {
                            // replace the #if node with the wrapped structure
                            node.ParentNode.ReplaceChild(blockResult.WrapperDiv, node);

                            // remove all nodes that were part of this conditional block
                            for (int j = 0; j < blockResult.NodesToRemove.Count; j++)
                            {
                                blockResult.NodesToRemove[j].Remove();
                            }

                            // refresh the list
                            allNodes = doc.DocumentNode.ChildNodes.ToList();
                            continue;
                        }
                    }
                }

                i++;
            }
        }

        /// <summary>
        /// process an if block and return a wrapper div with all conditional branches
        /// </summary>
        private ConditionalBlockResult? ProcessIfBlock(List<HtmlNode> nodes, int startIndex)
        {
            var doc = nodes[startIndex].OwnerDocument;
            var wrapperDiv = doc.CreateElement("div");
            wrapperDiv.SetAttributeValue("class", "conditional-block");

            var nodesToRemove = new List<HtmlNode>();
            var currentBranchNodes = new List<HtmlNode>();
            var currentCondition = "";
            var currentBranchType = "";
            var i = startIndex;
            var nestLevel = 0;

            while (i < nodes.Count)
            {
                var node = nodes[i];

                if (IsConditionalNode(node, out var condType, out var condition))
                {
                    if (condType == "if")
                    {
                        if (i == startIndex)
                        {
                            // first #if
                            currentCondition = condition;
                            currentBranchType = "if";
                            nodesToRemove.Add(node);
                        }
                        else
                        {
                            // nested #if
                            nestLevel++;
                            currentBranchNodes.Add(node);
                        }
                    }
                    else if (condType == "else if")
                    {
                        if (nestLevel == 0)
                        {
                            // wrap previous branch
                            WrapBranch(
                                doc,
                                wrapperDiv,
                                currentBranchNodes,
                                currentBranchType,
                                currentCondition
                            );
                            currentBranchNodes.Clear();

                            // start new else if branch
                            currentCondition = condition;
                            currentBranchType = "else if";
                            nodesToRemove.Add(node);
                        }
                        else
                        {
                            currentBranchNodes.Add(node);
                        }
                    }
                    else if (condType == "else")
                    {
                        if (nestLevel == 0)
                        {
                            // wrap previous branch
                            WrapBranch(
                                doc,
                                wrapperDiv,
                                currentBranchNodes,
                                currentBranchType,
                                currentCondition
                            );
                            currentBranchNodes.Clear();

                            // start else branch
                            currentBranchType = "else";
                            currentCondition = "";
                            nodesToRemove.Add(node);
                        }
                        else
                        {
                            currentBranchNodes.Add(node);
                        }
                    }
                    else if (condType == "end if")
                    {
                        if (nestLevel == 0)
                        {
                            // wrap last branch
                            WrapBranch(
                                doc,
                                wrapperDiv,
                                currentBranchNodes,
                                currentBranchType,
                                currentCondition
                            );
                            nodesToRemove.Add(node);
                            break;
                        }
                        else
                        {
                            nestLevel--;
                            currentBranchNodes.Add(node);
                        }
                    }
                }
                else
                {
                    // regular node, add to current branch
                    currentBranchNodes.Add(node);
                }

                i++;
            }

            if (nodesToRemove.Count > 0)
            {
                return new ConditionalBlockResult
                {
                    WrapperDiv = wrapperDiv,
                    NodesToRemove = nodesToRemove
                };
            }

            return null;
        }

        /// <summary>
        /// wrap a conditional branch in a div with appropriate Vue directive
        /// </summary>
        private static void WrapBranch(
            HtmlDocument doc,
            HtmlNode parent,
            List<HtmlNode> nodes,
            string branchType,
            string condition
        )
        {
            if (nodes.Count == 0)
                return;

            var branchDiv = doc.CreateElement("div");

            if (branchType == "if")
            {
                branchDiv.SetAttributeValue("v-if", condition);
            }
            else if (branchType == "else if")
            {
                branchDiv.SetAttributeValue("v-else-if", condition);
            }
            else if (branchType == "else")
            {
                branchDiv.SetAttributeValue("v-else", "");
            }

            foreach (var node in nodes)
            {
                branchDiv.AppendChild(node.Clone());
                // remove original node
                node.Remove();
            }

            parent.AppendChild(branchDiv);
        }

        /// <summary>
        /// check if a node is a conditional statement node
        /// </summary>
        private bool IsConditionalNode(
            HtmlNode node,
            out string conditionType,
            out string condition
        )
        {
            conditionType = "";
            condition = "";

            if (!node.Name.Equals("p", StringComparison.CurrentCultureIgnoreCase))
                return false;

            var condSpan = node.SelectSingleNode(".//span[@class='cond']");
            if (condSpan == null)
                return false;

            var text = condSpan.InnerText.Trim().ToLower();

            if (text.StartsWith("#if"))
            {
                // check if it's #if or part of #end if
                if (text == "#if")
                {
                    conditionType = "if";
                    // extract condition from the next span element
                    var eqSpan = node.SelectSingleNode(".//span[@class='eq']");
                    if (eqSpan != null)
                    {
                        condition = ExtractCondition(eqSpan);
                    }
                    return true;
                }
            }
            else if (text.Contains("#else if"))
            {
                conditionType = "else if";
                var eqSpan = node.SelectSingleNode(".//span[@class='eq']");
                if (eqSpan != null)
                {
                    condition = ExtractCondition(eqSpan);
                }
                return true;
            }
            else if (text == "#else")
            {
                conditionType = "else";
                return true;
            }
            else if (text.Contains("#end if"))
            {
                conditionType = "end if";
                return true;
            }

            return false;
        }

        /// <summary>
        /// extract condition expression from equation span and convert to JavaScript syntax
        /// </summary>
        private static string ExtractCondition(HtmlNode eqSpan)
        {
            var innerText = eqSpan.InnerText.Trim();
            // convert calcpad operators to JavaScript
            // ≡ -> ==
            // ≠ -> !=
            // ≤ -> <=
            // ≥ -> >=
            innerText = innerText
                .Replace("≡", "==")
                .Replace("≠", "!=")
                .Replace("≤", "<=")
                .Replace("≥", ">=")
                .Replace(" ", "");

            return innerText;
        }
    }
}
