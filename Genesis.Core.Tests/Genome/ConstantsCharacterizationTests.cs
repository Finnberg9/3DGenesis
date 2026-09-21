using Genesis.Core.Tests.Fixtures;
using Genesis.Core.Tests.TestKit;
using CoreGenome = Genesis.Core.Genome;

namespace Genesis.Core.Tests.Genome;

/// <summary>
/// Verifies every hand-typed constant in PartVocab/GenomeCaps/BrainConstants
/// matches the ACTUAL values exported by the running JS build (dumped by
/// the fixture generator directly from its PART/CAPS/PLAN/etc. objects, not
/// re-derived from reading the source), so a future edit to index.html's
/// numbers is caught here instead of silently drifting between the JS and
/// C# ports.
/// </summary>
public sealed class ConstantsCharacterizationTests
{
    [Fact]
    public void PartVocab_MatchesJsPartTable()
    {
        var jsPart = FixtureLoader.Data.Constants.Part;
        foreach (var (type, def) in CoreGenome.PartVocab.Parts)
        {
            string jsKey = type.ToString().ToUpperInvariant();
            Assert.True(jsPart.ContainsKey(jsKey), $"JS PART table has no entry for {jsKey}");
            var jsDef = jsPart[jsKey];
            Assert.Equal(jsDef.Upkeep, def.Upkeep, $"{jsKey}: upkeep");
            Assert.Equal(jsDef.MaxRepeat, def.MaxRepeat, $"{jsKey}: maxRepeat");
            Assert.Equal(jsDef.Singleton, def.Singleton, $"{jsKey}: singleton");
        }
        Assert.Equal(jsPart.Count, CoreGenome.PartVocab.Parts.Count, "part count mismatch: a type exists on one side but not the other");
    }

    [Fact]
    public void Singleton_MatchesJsSet()
    {
        var jsSingleton = FixtureLoader.Data.Constants.Singleton.Select(TestSupport.MapPartType).ToHashSet();
        Assert.Equal(jsSingleton.Count, CoreGenome.PartVocab.Singleton.Count);
        foreach (var t in jsSingleton) Assert.True(CoreGenome.PartVocab.Singleton.Contains(t), $"{t} missing from C# SINGLETON");
    }

    [Fact]
    public void MouthModes_MatchesJsList_InOrder()
    {
        var jsModes = FixtureLoader.Data.Constants.MouthModes;
        Assert.Equal(jsModes.Count, CoreGenome.PartVocab.MouthModes.Count);
        for (int i = 0; i < jsModes.Count; i++)
        {
            Assert.Equal(TestSupport.MapMouthMode(jsModes[i]), CoreGenome.PartVocab.MouthModes[i], $"index {i}");
        }
    }

    [Fact]
    public void Caps_MatchesJsCapsObject()
    {
        var caps = FixtureLoader.Data.Constants.Caps;
        Assert.Equal(caps["POP"], CoreGenome.GenomeCaps.Pop);
        Assert.Equal(caps["PLANTS"], CoreGenome.GenomeCaps.Plants);
        Assert.Equal(caps["GENOME_NODES"], CoreGenome.GenomeCaps.GenomeNodes);
        Assert.Equal(caps["PARTS_RENDERED"], CoreGenome.GenomeCaps.PartsRendered);
        Assert.Equal(caps["TREE_DEPTH"], CoreGenome.GenomeCaps.TreeDepth);
        Assert.Equal(caps["SPECIES"], CoreGenome.GenomeCaps.Species);
        Assert.Equal(caps["BRAIN_HID"], CoreGenome.GenomeCaps.BrainHid);
        Assert.Equal(caps["BRAIN_CONN"], CoreGenome.GenomeCaps.BrainConn);
        Assert.Equal(FixtureLoader.Data.Constants.MaxSegs, CoreGenome.GenomeCaps.MaxSegs);
        Assert.Equal(FixtureLoader.Data.Constants.MaxRadial, CoreGenome.GenomeCaps.MaxRadial);
    }

    [Fact]
    public void BodyPlanRange_MatchesJsPlanObject()
    {
        var plan = FixtureLoader.Data.Plan;
        Assert.Equal(plan["elong"][0], CoreGenome.BodyPlanRange.Elong.Min);
        Assert.Equal(plan["elong"][1], CoreGenome.BodyPlanRange.Elong.Max);
        Assert.Equal(plan["neck"][0], CoreGenome.BodyPlanRange.Neck.Min);
        Assert.Equal(plan["neck"][1], CoreGenome.BodyPlanRange.Neck.Max);
        Assert.Equal(plan["legLen"][0], CoreGenome.BodyPlanRange.LegLen.Min);
        Assert.Equal(plan["legLen"][1], CoreGenome.BodyPlanRange.LegLen.Max);
    }

    [Fact]
    public void BrainConstants_MatchIndexHtmlLiterals()
    {
        // IN/OUT/BIAS_ID/NOUT_BASE/BRAIN_HID0 and the gait/grab tuning
        // constants aren't exported by the JS module's API object (they're
        // module-private), so these are asserted directly against the
        // literals read from index.html (lines 323-354) rather than a JSON
        // fixture. If Finn ever changes these in the JS, this test will NOT
        // catch it automatically -- flagged here so a future run knows to
        // re-diff this block against the source if brain-shape tests start failing.
        Assert.Equal(26, CoreGenome.BrainConstants.In);
        Assert.Equal(8, CoreGenome.BrainConstants.Out);
        Assert.Equal(26, CoreGenome.BrainConstants.BiasId);
        Assert.Equal(100000, CoreGenome.BrainConstants.OutputBase);
        Assert.Equal(10, CoreGenome.BrainConstants.InitialHiddenCount);
        Assert.Equal(0.55, CoreGenome.BrainConstants.GaitBoost);
        Assert.Equal(0.85, CoreGenome.BrainConstants.GaitCost);
        Assert.Equal(0.55, CoreGenome.BrainConstants.GrabGain);
        Assert.Equal(0.30, CoreGenome.BrainConstants.GrabCost);
    }
}
