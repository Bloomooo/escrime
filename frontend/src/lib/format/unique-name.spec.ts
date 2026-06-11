import { describe, expect, it } from 'vitest';
import { forgeUniqueName } from './unique-name';

describe('forgeUniqueName', () => {
	it('should_keep_the_name_when_it_is_free', () => {
		expect(forgeUniqueName('zeus', ['Sir Galahad'])).toBe('zeus');
	});

	it('should_append_II_when_the_name_is_taken', () => {
		expect(forgeUniqueName('zeus', ['zeus'])).toBe('zeus II');
	});

	it('should_count_existing_roman_namesakes_when_numbering', () => {
		expect(forgeUniqueName('zeus', ['zeus', 'zeus II'])).toBe('zeus III');
		expect(forgeUniqueName('zeus', ['zeus', 'zeus II', 'zeus III'])).toBe('zeus IV');
	});

	it('should_ignore_non_roman_suffixes_when_matching_namesakes', () => {
		expect(forgeUniqueName('zeus', ['zeus le Grand', 'zeusette'])).toBe('zeus');
	});
});
