var assert = require('assert');

exports['Test 1'] = function (test) {
    assert.ok(true, "This shouldn't fail");
}

exports['Test 2'] = function (test) {
    assert.ok(1 === 1, "This shouldn't fail");
    assert.ok(false, "This should fail");
}