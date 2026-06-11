import { describe, expect, it } from 'vitest';
import { toRoman } from './roman';

describe('toRoman', () => {
	it.each([
		[1, 'I'],
		[2, 'II'],
		[4, 'IV'],
		[9, 'IX'],
		[14, 'XIV'],
		[40, 'XL'],
		[2026, 'MMXXVI']
	])('should_convert_%i_to_%s', (value, expected) => {
		expect(toRoman(value)).toBe(expected);
	});

	it('should_return_an_empty_string_when_the_value_is_not_a_positive_integer', () => {
		expect(toRoman(0)).toBe('');
		expect(toRoman(-3)).toBe('');
		expect(toRoman(2.5)).toBe('');
	});
});
