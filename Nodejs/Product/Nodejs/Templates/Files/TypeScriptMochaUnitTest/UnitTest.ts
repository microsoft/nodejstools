// placeholder declarations for 'describe' and 'it'
// if you want a more complete declaration add the
// Mocha declaration file from http://definitelytyped.org/
declare var describe: (description: string, spec: () => void) => any;
declare var it: (expectation: string, assertion: () => void) => any;

import assert = require('assert');

describe("Test Suite 1", () => {
    it("Test A", () => {
        assert.ok(true, "This shouldn't fail");
    });

    it("Test B", () => {
        assert.ok(1 === 1, "This shouldn't fail");
        assert.ok(false, "This should fail ts");
    });
});
