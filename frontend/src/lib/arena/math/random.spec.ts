import { describe, expect, it } from 'vitest';
import { fnv1a, mulberry32 } from './random';

describe('fnv1a', () => {
	it('should_return_the_offset_basis_when_hashing_an_empty_string', () => {
		expect(fnv1a('')).toBe(0x811c9dc5);
	});

	// Published FNV-1a 32-bit test vectors (Fowler/Noll/Vo reference).
	it.each([
		['a', 0xe40c292c],
		['foobar', 0xbf9cf968]
	])('should_match_the_published_vector_when_hashing_%s', (input, expected) => {
		expect(fnv1a(input)).toBe(expected);
	});

	it('should_produce_distinct_hashes_when_strings_differ', () => {
		expect(fnv1a('player-1:0')).not.toBe(fnv1a('player-1:1'));
	});
});

describe('mulberry32', () => {
	it('should_produce_the_same_sequence_when_seeded_identically', () => {
		const a = mulberry32(123456);
		const b = mulberry32(123456);

		const drawsA = [a(), a(), a(), a(), a()];
		const drawsB = [b(), b(), b(), b(), b()];

		expect(drawsA).toEqual(drawsB);
	});

	it('should_match_the_reference_sequence_when_seeded_with_1', () => {
		const next = mulberry32(1);

		expect([next(), next(), next(), next()]).toEqual([
			0.6270739405881613, 0.002735721180215478, 0.5274470399599522, 0.9810509674716741
		]);
	});

	it('should_match_the_reference_sequence_when_seeded_from_fnv1a', () => {
		const next = mulberry32(fnv1a('player-7:2'));

		expect([next(), next(), next()]).toEqual([
			0.2675954280421138, 0.5406125066801906, 0.7987681662198156
		]);
	});

	it('should_stay_within_the_unit_interval_when_drawing_many_values', () => {
		const next = mulberry32(987654321);

		for (let i = 0; i < 10_000; i++) {
			const value = next();
			expect(value).toBeGreaterThanOrEqual(0);
			expect(value).toBeLessThan(1);
		}
	});

	it('should_produce_different_sequences_when_seeds_differ', () => {
		const a = mulberry32(1);
		const b = mulberry32(2);

		expect([a(), a(), a()]).not.toEqual([b(), b(), b()]);
	});
});
