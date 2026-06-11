import { expect, test } from '@playwright/test';

test('should_display_the_arena_title_when_visiting_the_home', async ({ page }) => {
	await page.goto('/');

	await expect(page.getByRole('heading', { level: 1 })).toHaveText(
		'Le tournoi se regarde, il ne se consulte pas'
	);
});

const screens = [
	{ link: 'Combattants', heading: 'Combattants' },
	{ link: 'Duel', heading: 'Duel' },
	{ link: 'Reconstitution', heading: 'Reconstitution' },
	{ link: 'Classement', heading: 'Classement' }
] as const;

for (const { link, heading } of screens) {
	test(`should_reach_${heading}_when_following_the_nav`, async ({ page }) => {
		await page.goto('/');

		await page.getByRole('navigation').getByRole('link', { name: link }).click();

		await expect(page.getByRole('heading', { level: 1 })).toHaveText(heading);
	});
}
