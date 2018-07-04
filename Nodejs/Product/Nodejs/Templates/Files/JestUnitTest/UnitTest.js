describe('Test Suite 1', function () {
    it("Test 1 - This shouldn't fail", function () {
        expect(true).toBeTruthy();
    });

    it('Test 2 - This should fail', function () {
        expect(1 === 1).toBeTruthy(); // "This shouldn't fail"
        expect(false).toBeTruthy();
    });
});
