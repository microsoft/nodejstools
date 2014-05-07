import assert = require('assert');

export function Test1(test) {
    assert.ok(true, "This shouldn't fail");
}

export function Test2(test) {
    assert.ok(1 === 1, "This shouldn't fail");
    assert.ok(false, "This should fail");
}
