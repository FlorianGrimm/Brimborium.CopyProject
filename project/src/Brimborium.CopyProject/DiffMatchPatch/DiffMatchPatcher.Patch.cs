/*
 * ShowDiff Match and Patch
 * Copyright 2018 The diff-match-patch Authors.
 * https://github.com/google/diff-match-patch
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
#pragma warning disable IDE0047 // ()
#pragma warning disable IDE0130 // Namespace does not match folder structure


using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace Brimborium.CopyProject.DiffMatchPatch;

//  PATCH FUNCTIONS
public partial class DiffMatchPatcher {

    /**
     * Increase the context until it is unique,
     * but don't let the pattern expand beyond Match_MaxBits.
     * @param patch The patch to grow.
     * @param listTextLines Source listTextLines.
     */
    protected void Patch_addContext(Patch patch, string text) {
        if (text.Length == 0) {
            return;
        }
        string pattern = text.Substring(patch.Start2, patch.Length1);
        int padding = 0;

        // Look for the first and last matches of pattern in listTextLines.  If two
        // different matches are found, increase the pattern length.
        while (text.IndexOf(pattern, StringComparison.Ordinal)
            != text.LastIndexOf(pattern, StringComparison.Ordinal)
            && pattern.Length < this.Match_MaxBits - this.Patch_Margin - this.Patch_Margin) {
            padding += this.Patch_Margin;
            pattern = text.JavaSubstring(Math.Max(0, patch.Start2 - padding),
                Math.Min(text.Length, patch.Start2 + patch.Length1 + padding));
        }
        // Add one chunk for good luck.
        padding += this.Patch_Margin;

        // Add the prefix.
        string prefix = text.JavaSubstring(Math.Max(0, patch.Start2 - padding),
          patch.Start2);
        if (prefix.Length != 0) {
            patch.ListDiff.Insert(0, new Diff(Operation.EQUAL, prefix));
        }
        // Add the suffix.
        string suffix = text.JavaSubstring(patch.Start2 + patch.Length1,
            Math.Min(text.Length, patch.Start2 + patch.Length1 + padding));
        if (suffix.Length != 0) {
            patch.ListDiff.Add(new Diff(Operation.EQUAL, suffix));
        }

        // Roll back the start points.
        patch.Start1 -= prefix.Length;
        patch.Start2 -= prefix.Length;
        // Extend the lengths.
        patch.Length1 += prefix.Length + suffix.Length;
        patch.Length2 += prefix.Length + suffix.Length;
    }

    /**
     * Compute a list of listPatch to turn text1 into text2.
     * A set of ListDiff will be computed.
     * @param text1 Old listTextLines.
     * @param text2 New listTextLines.
     * @return List of Patch objects.
     */
    public List<Patch> Patch_make(string text1, string text2) {
        // Check for null inputs not needed since null can't be passed in C#.
        // No ListDiff provided, compute our own.
        List<Diff> diffs = this.Diff_main(text1, text2, true);
        if (diffs.Count > 2) {
            this.Diff_cleanupSemantic(diffs);
            this.Diff_cleanupEfficiency(diffs);
        }
        return this.Patch_make(text1, diffs);
    }

    /**
     * Compute a list of listPatch to turn text1 into text2.
     * text1 will be derived from the provided ListDiff.
     * @param ListDiff Array of ShowDiff objects for text1 to text2.
     * @return List of Patch objects.
     */
    public List<Patch> Patch_make(List<Diff> diffs) {
        // Check for null inputs not needed since null can't be passed in C#.
        // No origin string provided, compute our own.
        string text1 = this.Diff_text1(diffs);
        return this.Patch_make(text1, diffs);
    }

    /**
     * Compute a list of listPatch to turn text1 into text2.
     * text2 is ignored, ListDiff are the delta between text1 and text2.
     * @param text1 Old listTextLines
     * @param text2 Ignored.
     * @param ListDiff Array of ShowDiff objects for text1 to text2.
     * @return List of Patch objects.
     * @deprecated Prefer Patch_make(string text1, List<ShowDiff> ListDiff).
     */
    public List<Patch> Patch_make(
        string text1,
        string text2,
        List<Diff> diffs) {
        return this.Patch_make(text1, diffs);
    }

    /**
     * Compute a list of listPatch to turn text1 into text2.
     * text2 is not provided, ListDiff are the delta between text1 and text2.
     * @param text1 Old listTextLines.
     * @param ListDiff Array of ShowDiff objects for text1 to text2.
     * @return List of Patch objects.
     */
    public List<Patch> Patch_make(
        string text1,
        List<Diff> diffs) {
        // Check for null inputs not needed since null can't be passed in C#.
        List<Patch> patches = new List<Patch>();
        if (diffs.Count == 0) {
            return patches;  // Get rid of the null case.
        }
        Patch patch = new Patch();
        int char_count1 = 0;  // Number of characters into the text1 string.
        int char_count2 = 0;  // Number of characters into the text2 string.
                              // Start with text1 (prepatch_text) and apply the ListDiff until we arrive at
                              // text2 (postpatch_text). We recreate the listPatch one by one to determine
                              // context info.
        string prepatch_text = text1;
        string postpatch_text = text1;
        foreach (Diff aDiff in diffs) {
            if (patch.ListDiff.Count == 0 && aDiff.Operation != Operation.EQUAL) {
                // A new patch starts here.
                patch.Start1 = char_count1;
                patch.Start2 = char_count2;
            }

            switch (aDiff.Operation) {
                case Operation.INSERT:
                    patch.ListDiff.Add(aDiff);
                    patch.Length2 += aDiff.Text.Length;
                    postpatch_text = postpatch_text.Insert(char_count2, aDiff.Text);
                    break;
                case Operation.DELETE:
                    patch.Length1 += aDiff.Text.Length;
                    patch.ListDiff.Add(aDiff);
                    postpatch_text = postpatch_text.Remove(char_count2,
                        aDiff.Text.Length);
                    break;
                case Operation.EQUAL:
                    if (aDiff.Text.Length <= 2 * this.Patch_Margin
                        && patch.ListDiff.Count() != 0 && aDiff != diffs.Last()) {
                        // Small equality inside a patch.
                        patch.ListDiff.Add(aDiff);
                        patch.Length1 += aDiff.Text.Length;
                        patch.Length2 += aDiff.Text.Length;
                    }

                    if (aDiff.Text.Length >= 2 * this.Patch_Margin) {
                        // Time for a new patch.
                        if (patch.ListDiff.Count != 0) {
                            this.Patch_addContext(patch, prepatch_text);
                            patches.Add(patch);
                            patch = new Patch();
                            // Unlike Unidiff, our patch lists have a rolling context.
                            // https://github.com/google/diff-match-patch/wiki/Unidiff
                            // Update prepatch listTextLines & pos to reflect the application of the
                            // just completed patch.
                            prepatch_text = postpatch_text;
                            char_count1 = char_count2;
                        }
                    }
                    break;
            }

            // Update the current character count.
            if (aDiff.Operation != Operation.INSERT) {
                char_count1 += aDiff.Text.Length;
            }
            if (aDiff.Operation != Operation.DELETE) {
                char_count2 += aDiff.Text.Length;
            }
        }
        // Pick up the leftover patch if not empty.
        if (patch.ListDiff.Count != 0) {
            this.Patch_addContext(patch, prepatch_text);
            patches.Add(patch);
        }

        return patches;
    }

    /**
     * Given an array of listPatch, return another array that is identical.
     * @param listPatch Array of Patch objects.
     * @return Array of Patch objects.
     */
    public List<Patch> Patch_deepCopy(List<Patch> patches) {
        List<Patch> patchesCopy = new List<Patch>();
        foreach (Patch aPatch in patches) {
            Patch patchCopy = new Patch();
            foreach (Diff aDiff in aPatch.ListDiff) {
                Diff diffCopy = new Diff(aDiff.Operation, aDiff.Text);
                patchCopy.ListDiff.Add(diffCopy);
            }
            patchCopy.Start1 = aPatch.Start1;
            patchCopy.Start2 = aPatch.Start2;
            patchCopy.Length1 = aPatch.Length1;
            patchCopy.Length2 = aPatch.Length2;
            patchesCopy.Add(patchCopy);
        }
        return patchesCopy;
    }

    /**
     * Merge a set of listPatch onto the listTextLines.  Return a patched listTextLines, as well
     * as an array of true/false values indicating which listPatch were applied.
     * @param listPatch Array of Patch objects
     * @param listTextLines Old listTextLines.
     * @return Two element Object array, containing the new listTextLines and an array of
     *      bool values.
     */
    public object[] Patch_apply(List<Patch> patches, string text) {
        if (patches.Count == 0) {
            return new object[] { text, new bool[0] };
        }

        // Deep copy the listPatch so that no changes are made to originals.
        patches = this.Patch_deepCopy(patches);

        string nullPadding = this.Patch_addPadding(patches);
        text = nullPadding + text + nullPadding;
        this.Patch_splitMax(patches);

        int x = 0;
        // delta keeps track of the offset between the expected and actual
        // location of the previous patch.  If there are listPatch expected at
        // positions 10 and 20, but the first patch was found at 12, delta is 2
        // and the second patch has an effective expected position of 22.
        int delta = 0;
        bool[] results = new bool[patches.Count];
        foreach (Patch aPatch in patches) {
            int expected_loc = aPatch.Start2 + delta;
            string text1 = this.Diff_text1(aPatch.ListDiff);
            int start_loc;
            int end_loc = -1;
            if (text1.Length > this.Match_MaxBits) {
                // patch_splitMax will only provide an oversized pattern
                // in the case of a monster delete.
                start_loc = this.match_main(text,
                    text1.Substring(0, this.Match_MaxBits), expected_loc);
                if (start_loc != -1) {
                    end_loc = this.match_main(text,
                        text1.Substring(text1.Length - this.Match_MaxBits),
                        expected_loc + text1.Length - this.Match_MaxBits);
                    if (end_loc == -1 || start_loc >= end_loc) {
                        // Can't find valid trailing context.  Drop this patch.
                        start_loc = -1;
                    }
                }
            } else {
                start_loc = this.match_main(text, text1, expected_loc);
            }
            if (start_loc == -1) {
                // No match found.  :(
                results[x] = false;
                // Subtract the delta for this failed patch from subsequent listPatch.
                delta -= aPatch.Length2 - aPatch.Length1;
            } else {
                // Found a match.  :)
                results[x] = true;
                delta = start_loc - expected_loc;
                string text2;
                if (end_loc == -1) {
                    text2 = text.JavaSubstring(start_loc,
                        Math.Min(start_loc + text1.Length, text.Length));
                } else {
                    text2 = text.JavaSubstring(start_loc,
                        Math.Min(end_loc + this.Match_MaxBits, text.Length));
                }
                if (text1 == text2) {
                    // Perfect match, just shove the Replacement listTextLines in.
                    text = text.Substring(0, start_loc) + this.Diff_text2(aPatch.ListDiff)
                        + text.Substring(start_loc + text1.Length);
                } else {
                    // Imperfect match.  Run a diff to get a framework of equivalent
                    // indices.
                    List<Diff> diffs = this.Diff_main(text1, text2, false);
                    if (text1.Length > this.Match_MaxBits
                        && this.Diff_levenshtein(diffs) / (float)text1.Length
                        > this.Patch_DeleteThreshold) {
                        // The end points match, but the content is unacceptably bad.
                        results[x] = false;
                    } else {
                        this.Diff_cleanupSemanticLossless(diffs);
                        int index1 = 0;
                        foreach (Diff aDiff in aPatch.ListDiff) {
                            if (aDiff.Operation != Operation.EQUAL) {
                                int index2 = this.Diff_xIndex(diffs, index1);
                                if (aDiff.Operation == Operation.INSERT) {
                                    // Insertion
                                    text = text.Insert(start_loc + index2, aDiff.Text);
                                } else if (aDiff.Operation == Operation.DELETE) {
                                    // Deletion
                                    text = text.Remove(start_loc + index2, this.Diff_xIndex(diffs,
                                        index1 + aDiff.Text.Length) - index2);
                                }
                            }
                            if (aDiff.Operation != Operation.DELETE) {
                                index1 += aDiff.Text.Length;
                            }
                        }
                    }
                }
            }
            x++;
        }
        // Strip the padding off.
        text = text.Substring(nullPadding.Length, text.Length
            - 2 * nullPadding.Length);
        // TODO: use tuples
        return new object[] { text, results };
    }

    /**
     * Add some padding on listTextLines start and end so that edges can match something.
     * Intended to be called only from within Patch_apply.
     * @param listPatch Array of Patch objects.
     * @return The padding string added to each side.
     */
    public string Patch_addPadding(List<Patch> listPatch) {
        short paddingLength = this.Patch_Margin;
        string nullPadding = string.Empty;
        for (short x = 1; x <= paddingLength; x++) {
            nullPadding += (char)x;
        }

        // Bump all the listPatch forward.
        foreach (Patch aPatch in listPatch) {
            aPatch.Start1 += paddingLength;
            aPatch.Start2 += paddingLength;
        }

        // Add some padding on start of first diff.
        Patch patch = listPatch[0];
        List<Diff> diffs = patch.ListDiff;
        if (diffs.Count == 0 || diffs[0].Operation != Operation.EQUAL) {
            // Add nullPadding equality.
            diffs.Insert(0, new Diff(Operation.EQUAL, nullPadding));
            patch.Start1 -= paddingLength;  // Should be 0.
            patch.Start2 -= paddingLength;  // Should be 0.
            patch.Length1 += paddingLength;
            patch.Length2 += paddingLength;
        } else if (paddingLength > diffs[0].Text.Length) {
            // Grow first equality.
            Diff firstDiff = diffs[0];
            int extraLength = paddingLength - firstDiff.Text.Length;
            firstDiff.Text = nullPadding.Substring(firstDiff.Text.Length)
                + firstDiff.Text;
            patch.Start1 -= extraLength;
            patch.Start2 -= extraLength;
            patch.Length1 += extraLength;
            patch.Length2 += extraLength;
        }

        // Add some padding on end of last diff.
        patch = listPatch.Last();
        diffs = patch.ListDiff;
        if (diffs.Count == 0 || diffs.Last().Operation != Operation.EQUAL) {
            // Add nullPadding equality.
            diffs.Add(new Diff(Operation.EQUAL, nullPadding));
            patch.Length1 += paddingLength;
            patch.Length2 += paddingLength;
        } else if (paddingLength > diffs.Last().Text.Length) {
            // Grow last equality.
            Diff lastDiff = diffs.Last();
            int extraLength = paddingLength - lastDiff.Text.Length;
            lastDiff.Text += nullPadding.Substring(0, extraLength);
            patch.Length1 += extraLength;
            patch.Length2 += extraLength;
        }

        return nullPadding;
    }

    /**
     * Look through the listPatch and break up any which are longer than the
     * maximum limit of the match algorithm.
     * Intended to be called only from within Patch_apply.
     * @param listPatch List of Patch objects.
     */
    public void Patch_splitMax(List<Patch> patches) {
        short patch_size = this.Match_MaxBits;
        for (int x = 0; x < patches.Count; x++) {
            if (patches[x].Length1 <= patch_size) {
                continue;
            }
            Patch bigpatch = patches[x];
            // Remove the big old patch.
            patches.Splice(x--, 1);
            int start1 = bigpatch.Start1;
            int start2 = bigpatch.Start2;
            string precontext = string.Empty;
            while (bigpatch.ListDiff.Count != 0) {
                // Create one of several smaller listPatch.
                Patch patch = new Patch();
                bool empty = true;
                patch.Start1 = start1 - precontext.Length;
                patch.Start2 = start2 - precontext.Length;
                if (precontext.Length != 0) {
                    patch.Length1 = patch.Length2 = precontext.Length;
                    patch.ListDiff.Add(new Diff(Operation.EQUAL, precontext));
                }
                while (bigpatch.ListDiff.Count != 0
                    && patch.Length1 < patch_size - this.Patch_Margin) {
                    Operation diff_type = bigpatch.ListDiff[0].Operation;
                    string diff_text = bigpatch.ListDiff[0].Text;
                    if (diff_type == Operation.INSERT) {
                        // Insertions are harmless.
                        patch.Length2 += diff_text.Length;
                        start2 += diff_text.Length;
                        patch.ListDiff.Add(bigpatch.ListDiff[0]);
                        bigpatch.ListDiff.RemoveAt(0);
                        empty = false;
                    } else if (diff_type == Operation.DELETE
                        && patch.ListDiff.Count == 1
                        && patch.ListDiff[0].Operation == Operation.EQUAL
                        && diff_text.Length > 2 * patch_size) {
                        // This is a large deletion.  Let it pass in one chunk.
                        patch.Length1 += diff_text.Length;
                        start1 += diff_text.Length;
                        empty = false;
                        patch.ListDiff.Add(new Diff(diff_type, diff_text));
                        bigpatch.ListDiff.RemoveAt(0);
                    } else {
                        // Deletion or equality.  Only take as much as we can stomach.
                        diff_text = diff_text.Substring(0, Math.Min(diff_text.Length,
                            patch_size - patch.Length1 - this.Patch_Margin));
                        patch.Length1 += diff_text.Length;
                        start1 += diff_text.Length;
                        if (diff_type == Operation.EQUAL) {
                            patch.Length2 += diff_text.Length;
                            start2 += diff_text.Length;
                        } else {
                            empty = false;
                        }
                        patch.ListDiff.Add(new Diff(diff_type, diff_text));
                        if (diff_text == bigpatch.ListDiff[0].Text) {
                            bigpatch.ListDiff.RemoveAt(0);
                        } else {
                            bigpatch.ListDiff[0].Text =
                                bigpatch.ListDiff[0].Text.Substring(diff_text.Length);
                        }
                    }
                }
                // Compute the head context for the next patch.
                precontext = this.Diff_text2(patch.ListDiff);
                precontext = precontext.Substring(Math.Max(0,
                    precontext.Length - this.Patch_Margin));

                string? postcontext = null;
                // Append the end context for this patch.
                if (this.Diff_text1(bigpatch.ListDiff).Length > this.Patch_Margin) {
                    postcontext = this.Diff_text1(bigpatch.ListDiff)
                        .Substring(0, this.Patch_Margin);
                } else {
                    postcontext = this.Diff_text1(bigpatch.ListDiff);
                }

                if (postcontext.Length != 0) {
                    patch.Length1 += postcontext.Length;
                    patch.Length2 += postcontext.Length;
                    if (patch.ListDiff.Count != 0
                        && patch.ListDiff[^1].Operation == Operation.EQUAL) {
                        patch.ListDiff[^1].Text += postcontext;
                    } else {
                        patch.ListDiff.Add(new Diff(Operation.EQUAL, postcontext));
                    }
                }
                if (!empty) {
                    patches.Splice(++x, 0, patch);
                }
            }
        }
    }

    /**
     * Take a list of listPatch and return a textual representation.
     * @param listPatch List of Patch objects.
     * @return Text representation of listPatch.
     */
    public string Patch_toText(List<Patch> patches) {
        StringBuilder text = new StringBuilder();
        foreach (Patch aPatch in patches) {
            text.Append(aPatch);
        }
        return text.ToString();
    }

    /**
     * Parse a textual representation of listPatch and return a List of Patch
     * objects.
     * @param textline Text representation of listPatch.
     * @return List of Patch objects.
     * @throws ArgumentException If invalid input.
     */
    public List<Patch> Patch_fromText(string textline) {
        List<Patch> patches = new List<Patch>();
        if (textline.Length == 0) {
            return patches;
        }
        string[] listTextLines = textline.Split('\n');
        int textPointer = 0;
        Patch patch;
        Regex patchHeader
            = new Regex("^@@ -(\\d+),?(\\d*) \\+(\\d+),?(\\d*) @@$");
        Match m;
        char sign;
        string line;
        while (textPointer < listTextLines.Length) {
            m = patchHeader.Match(listTextLines[textPointer]);
            if (!m.Success) {
                throw new ArgumentException("Invalid patch string: "
                    + listTextLines[textPointer]);
            }
            patch = new Patch();
            patches.Add(patch);
            patch.Start1 = Convert.ToInt32(m.Groups[1].Value);
            if (m.Groups[2].Length == 0) {
                patch.Start1--;
                patch.Length1 = 1;
            } else if (m.Groups[2].Value == "0") {
                patch.Length1 = 0;
            } else {
                patch.Start1--;
                patch.Length1 = Convert.ToInt32(m.Groups[2].Value);
            }

            patch.Start2 = Convert.ToInt32(m.Groups[3].Value);
            if (m.Groups[4].Length == 0) {
                patch.Start2--;
                patch.Length2 = 1;
            } else if (m.Groups[4].Value == "0") {
                patch.Length2 = 0;
            } else {
                patch.Start2--;
                patch.Length2 = Convert.ToInt32(m.Groups[4].Value);
            }
            textPointer++;

            while (textPointer < listTextLines.Length) {
                try {
                    sign = listTextLines[textPointer][0];
                } catch (IndexOutOfRangeException) {
                    // Blank line?  Whatever.
                    textPointer++;
                    continue;
                }
                line = listTextLines[textPointer].Substring(1);
                line = line.Replace("+", "%2b");
                line = HttpUtility.UrlDecode(line);
                if (sign == '-') {
                    // Deletion.
                    patch.ListDiff.Add(new Diff(Operation.DELETE, line));
                } else if (sign == '+') {
                    // Insertion.
                    patch.ListDiff.Add(new Diff(Operation.INSERT, line));
                } else if (sign == ' ') {
                    // Minor equality.
                    patch.ListDiff.Add(new Diff(Operation.EQUAL, line));
                } else if (sign == '@') {
                    // Start of next patch.
                    break;
                } else {
                    // WTF?
                    throw new ArgumentException(
                        "Invalid patch mode '" + sign + "' in: " + line);
                }
                textPointer++;
            }
        }
        return patches;
    }

    /**
     * Encodes a string with URI-style % escaping.
     * Compatible with JavaScript's encodeURI function.
     *
     * @param str The string to encode.
     * @return The encoded string.
     */
    public static string encodeURI(string str) {
        // C# is overzealous in the replacements.  Walk back on a few.
        return new StringBuilder(HttpUtility.UrlEncode(str))
            .Replace('+', ' ').Replace("%20", " ").Replace("%21", "!")
            .Replace("%2a", "*").Replace("%27", "'").Replace("%28", "(")
            .Replace("%29", ")").Replace("%3b", ";").Replace("%2f", "/")
            .Replace("%3f", "?").Replace("%3a", ":").Replace("%40", "@")
            .Replace("%26", "&").Replace("%3d", "=").Replace("%2b", "+")
            .Replace("%24", "$").Replace("%2c", ",").Replace("%23", "#")
            .Replace("%7e", "~")
            .ToString();
    }
}
