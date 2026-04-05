namespace EventModeling

/// Named groupings of slices rendered as labeled header rows above the grid.
/// Each level gets its own background color for visual navigation.
///
/// The cadence of nesting:
///   Flow     — short sequence of slices for one focused interaction
///              e.g. "Login", "Add to Cart"
///   Workflow — series of Flows or nested Workflows for a larger process
///              e.g. "Checkout", "Order Fulfillment"
///              Workflows can contain other Workflows — no fixed depth limit
///   Area     — top-level section grouping related Workflows
///              e.g. "Shopping", "Account Management"
///
/// Vocabulary is shared between business and technical audiences.
/// All three terms work naturally in conversation without signaling
/// a specific methodology or technical framework.
///
/// Terminology is intentionally minimal — expand only when a genuine
/// gap is felt through real usage, not in anticipation of one.
type Grouping =
    | Flow     of name: string * slices: SliceRef list
    | Workflow of name: string * children: Grouping list
    | Area     of name: string * workflows: Grouping list
