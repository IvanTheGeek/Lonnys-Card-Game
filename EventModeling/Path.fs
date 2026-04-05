namespace EventModeling

/// A Path is a specific, ordered flow of interactions through a system.
/// The Event Model captures one Path at a time.
/// Multiple Paths may exist and each is modeled independently.
///
/// As Paths accumulate, their GWT example data aggregates into:
///   - a complete test suite covering the full range of system behavior
///   - a usage reference feeding tutorial and onboarding material
///
/// Nothing is written twice — model and tests are the same artifact.
type Path = {
    Name:        string
    Description: string
    Slices:      SliceRef list
}
