var test = require("tape");

test("Test A", function (t) {
    t.plan(1);
    t.ok(true, "This shouldn't fail");
});

test("Test B", function (t) {
    t.plan(2);
    t.ok(true, "This shouldn't fail");
    t.equal(1, 2, "This should fail");
});
