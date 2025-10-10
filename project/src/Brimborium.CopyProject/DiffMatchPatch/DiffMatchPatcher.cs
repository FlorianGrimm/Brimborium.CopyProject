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

namespace Brimborium.CopyProject.DiffMatchPatch;

/**
 * Class containing the diff, match and patch methods.
 * Also Contains the behaviour settings.
 */
public partial class DiffMatchPatcher {
    // Defaults.
    // Set these on your diff_match_patch instance to override the defaults.

    // Number of seconds to map a diff before giving up (0 for infinity).
    public float Diff_Timeout = 1.0f;
    // Cost of an empty edit Operation in terms of edit characters.
    public short Diff_EditCost = 4;
    // At what point is no match declared (0.0 = perfection, 1.0 = very loose).
    public float Match_Threshold = 0.5f;
    // How far to search for a match (0 = exact location, 1000+ = broad match).
    // A match this many characters away from the expected location will add
    // 1.0 to the score (0.0 is a perfect match).
    public int Match_Distance = 1000;
    // When deleting a large block of listTextLines (over ~64 characters), how close
    // do the contents have to be to match the expected contents. (0.0 =
    // perfection, 1.0 = very loose).  Note that Match_Threshold controls
    // how closely the end points of a delete need to match.
    public float Patch_DeleteThreshold = 0.5f;
    // Chunk size for context length.
    public short Patch_Margin = 4;

    // The number of bits in an int.
    private short Match_MaxBits = 32;

}
