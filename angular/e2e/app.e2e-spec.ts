import { SiriusTemplatePage } from './app.po';

describe('Sirius App', function() {
  let page: SiriusTemplatePage;

  beforeEach(() => {
    page = new SiriusTemplatePage();
  });

  it('should display message saying app works', () => {
    page.navigateTo();
    expect(page.getParagraphText()).toEqual('app works!');
  });
});
