/*
 * Diff Match and Patch
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

namespace Brimborium.CopyProject.DiffMatchPatch;


/**
 * Class representing one patch operation.
 */
public class Patch {
    public List<Diff> ListDiff = new List<Diff>();
    public int Start1;
    public int Start2;
    public int Length1;
    public int Length2;

    /**
     * Emulate GNU diff's format.
     * Header: @@ -382,8 +481,9 @@
     * Indices are printed as 1-based, not 0-based.
     * @return The GNU diff string.
     */
    public override string ToString() {
        string coords1, coords2;
        if (this.Length1 == 0) {
            coords1 = this.Start1 + ",0";
        } else if (this.Length1 == 1) {
            coords1 = Convert.ToString(this.Start1 + 1);
        } else {
            coords1 = (this.Start1 + 1) + "," + this.Length1;
        }
        if (this.Length2 == 0) {
            coords2 = this.Start2 + ",0";
        } else if (this.Length2 == 1) {
            coords2 = Convert.ToString(this.Start2 + 1);
        } else {
            coords2 = (this.Start2 + 1) + "," + this.Length2;
        }
        StringBuilder text = new StringBuilder();
        text.Append("@@ -").Append(coords1).Append(" +").Append(coords2)
            .Append(" @@\n");
        // Escape the body of the patch with %xx notation.
        foreach (Diff aDiff in this.ListDiff) {
            switch (aDiff.operation) {
                case Operation.INSERT:
                    text.Append('+');
                    break;
                case Operation.DELETE:
                    text.Append('-');
                    break;
                case Operation.EQUAL:
                    text.Append(' ');
                    break;
            }

            text.Append(DiffMatchPatcher.encodeURI(aDiff.Text)).Append('\n');
        }
        return text.ToString();
    }
}
