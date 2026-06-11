import { describe, expect, it } from 'vitest';
import { initials } from './initials';

describe('initials', () => {
	it.each([
		['Sir Galahad', 'SG'],
		['Aldric de Fervane', 'AD'],
		['Morgane', 'M'],
		['  lancelot   du  lac ', 'LD']
	])('should_take_the_first_letters_of_the_first_two_words_for_%s', (name, expected) => {
		expect(initials(name)).toBe(expected);
	});
});
