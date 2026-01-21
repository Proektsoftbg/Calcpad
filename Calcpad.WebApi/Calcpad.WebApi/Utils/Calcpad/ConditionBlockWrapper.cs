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

    class ConditionalBranchSegment
    {
        public string BranchType { get; set; } = "";
        public string Condition { get; set; } = "";
        public List<HtmlNode> ContentNodes { get; set; } = new();
    }

    public class ConditionBlockWrapper(HtmlDocument doc)
    {
        /// <summary>
        /// process conditional blocks and wrap them in divs with v-if/v-else-if/v-else attributes
        /// </summary>
        /// <param name="doc"></param>
        public void ProcessConditionalBlocks()
        {
            ProcessConditionalBlocksInContainer(doc.DocumentNode);
        }

        /// <summary>
        /// recursively process conditional blocks within a container node.
        /// </summary>
        private void ProcessConditionalBlocksInContainer(HtmlNode container)
        {
            if (container.ChildNodes == null || container.ChildNodes.Count == 0)
                return;

            var siblings = container.ChildNodes.ToList();
            var i = 0;

            while (i < siblings.Count)
            {
                var node = siblings[i];
                if (IsConditionalNode(node, out var condType, out _) && condType == "if")
                {
                    var blockResult = TryBuildIfBlockWrapper(siblings, i);
                    if (blockResult != null)
                    {
                        // replace the #if node with the wrapped structure
                        container.ReplaceChild(blockResult.WrapperDiv, node);

                        // remove marker nodes (#if/#else if/#else/#end if) from DOM
                        foreach (var marker in blockResult.NodesToRemove)
                        {
                            if (marker.ParentNode != null)
                                marker.Remove();
                        }

                        // refresh sibling list after modifications
                        siblings = container.ChildNodes.ToList();
                        continue;
                    }
                }

                i++;
            }

            // recurse after current container is stabilized
            var elementChildren = container
                .ChildNodes.Where(n => n.NodeType == HtmlNodeType.Element)
                .ToList();

            foreach (var child in elementChildren)
            {
                ProcessConditionalBlocksInContainer(child);
            }
        }

        /// <summary>
        /// build a wrapper for an if/else-if/else block starting at startIndex.
        /// This method is two-phase: first scan to ensure a complete block exists, then mutate DOM by moving nodes.
        /// </summary>
        private ConditionalBlockResult? TryBuildIfBlockWrapper(
            IList<HtmlNode> siblings,
            int startIndex
        )
        {
            if (startIndex < 0 || startIndex >= siblings.Count)
                return null;

            var startNode = siblings[startIndex];
            if (
                !IsConditionalNode(startNode, out var startType, out var startCondition)
                || startType != "if"
            )
                return null;

            var segments = new List<ConditionalBranchSegment>();
            var markerNodesToRemove = new List<HtmlNode> { startNode };

            var currentSegment = new ConditionalBranchSegment
            {
                BranchType = "if",
                Condition = startCondition,
            };

            var nestLevel = 0;
            var foundEnd = false;

            for (var i = startIndex + 1; i < siblings.Count; i++)
            {
                var node = siblings[i];

                if (IsConditionalNode(node, out var condType, out var condition))
                {
                    if (condType == "if")
                    {
                        // nested #if starts a deeper level; treat node as content
                        nestLevel++;
                        currentSegment.ContentNodes.Add(node);
                        continue;
                    }

                    if (condType == "end if")
                    {
                        if (nestLevel == 0)
                        {
                            // close current block
                            segments.Add(currentSegment);
                            markerNodesToRemove.Add(node);
                            foundEnd = true;
                            break;
                        }

                        // close a nested level; treat as content
                        nestLevel--;
                        currentSegment.ContentNodes.Add(node);
                        continue;
                    }

                    if ((condType == "else if" || condType == "else") && nestLevel == 0)
                    {
                        // switch branch at top-level of this if-block
                        segments.Add(currentSegment);

                        currentSegment = new ConditionalBranchSegment
                        {
                            BranchType = condType,
                            Condition = condType == "else" ? "" : condition,
                        };

                        markerNodesToRemove.Add(node);
                        continue;
                    }

                    // else/else-if inside nested block: treat as content
                    currentSegment.ContentNodes.Add(node);
                    continue;
                }

                // regular node
                currentSegment.ContentNodes.Add(node);
            }

            if (!foundEnd)
                return null;

            var ownerDoc = startNode.OwnerDocument;
            if (ownerDoc == null)
                return null;

            var wrapperDiv = ownerDoc.CreateElement("div");
            wrapperDiv.SetAttributeValue("class", "conditional-block");

            foreach (var segment in segments)
            {
                var branchDiv = ownerDoc.CreateElement("div");
                switch (segment.BranchType)
                {
                    case "if":
                        branchDiv.SetAttributeValue("v-if", segment.Condition);
                        break;
                    case "else if":
                        branchDiv.SetAttributeValue("v-else-if", segment.Condition);
                        break;
                    case "else":
                        branchDiv.SetAttributeValue("v-else", "");
                        break;
                }

                // move nodes into branch div, preserving order
                foreach (var contentNode in segment.ContentNodes.ToList())
                {
                    contentNode.Remove();
                    branchDiv.AppendChild(contentNode);
                }

                wrapperDiv.AppendChild(branchDiv);
            }

            return new ConditionalBlockResult
            {
                WrapperDiv = wrapperDiv,
                NodesToRemove = markerNodesToRemove,
            };
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
