import { toRoman } from './roman';

const ROMAN_SUFFIX = /^[IVXLCDM]+$/;

// "zeus" taken gives "zeus II", then "zeus III": namesakes are numbered like dynasties.
export function forgeUniqueName(base: string, takenNames: string[]): string {
	const namesakes = takenNames.filter(
		(name) =>
			name === base ||
			(name.startsWith(`${base} `) && ROMAN_SUFFIX.test(name.slice(base.length + 1)))
	);
	return namesakes.length === 0 ? base : `${base} ${toRoman(namesakes.length + 1)}`;
}
