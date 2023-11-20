/** @odoo-module */

import wTourUtils from 'website.tour_utils';

wTourUtils.registerWebsitePreviewTour('snippet_image_gallery', {
    test: true,
    url: '/',
    edition: true,
}, [
    wTourUtils.dragNDrop({id: 's_image_gallery', name: 'Images Wall'}),
    ...wTourUtils.clickOnSave(),
    {
        content: 'Click on an image of the Image Wall',
        trigger: 'iframe .s_image_gallery img',
        run: 'click',
    },
    {
        content: 'Check that the modal has opened properly',
        trigger: 'iframe .s_gallery_lightbox img',
        isCheck: true,
    },
]);

wTourUtils.registerWebsitePreviewTour("snippet_image_gallery_remove", {
    test: true,
    url: "/",
    edition: true,
}, [
    wTourUtils.dragNDrop({
        id: "s_image_gallery",
        name: "Image Gallery",
}), wTourUtils.clickOnSnippet({
    id: 's_image_gallery',
    name: 'Image Gallery',
}), {
    content: "Click on Remove all",
    trigger: "we-button:has(div:contains('Remove all'))",
}, {
    content: "Click on Add Images",
    trigger: "iframe span:contains('Add Images')",
}, {
    content: "Click on the first new image",
    trigger: ".o_select_media_dialog img[title='s_default_image.jpg']",
}, {
    content: "Click on the second new image",
    trigger: ".o_select_media_dialog img[title='s_default_image2.jpg']",
},
    wTourUtils.addMedia(),
   {
    content: "Click on the image of the Image Gallery snippet",
    trigger: "iframe .s_image_gallery .carousel-item.active  img",
}, {
    content: "Check that the Snippet Editor of the clicked image has been loaded",
    trigger: "we-customizeblock-options span:contains('Image'):not(:contains('Image Gallery'))",
    isCheck: true,
}, {
    content: "Click on Remove Block",
    trigger: ".o_we_customize_panel we-title:has(span:contains('Image Gallery')) we-button[title='Remove Block']",
}, {
    content: "Check that the Image Gallery snippet has been removed",
    trigger: "iframe #wrap:not(:has(.s_image_gallery))",
    isCheck: true,
}]);

wTourUtils.registerWebsitePreviewTour("snippet_image_gallery_reorder", {
    test: true,
    url: "/",
    edition: true,
}, [
    wTourUtils.dragNDrop({
        id: "s_image_gallery",
        name: "Image Gallery",
    }),
{
    content: "Click on the first image of the snippet",
    trigger: "iframe .s_image_gallery .carousel-item.active img",
},
    wTourUtils.changeOption('ImageTools', 'we-select:contains("Filter") we-toggler'),
    wTourUtils.changeOption('ImageTools', '[data-gl-filter="blur"]'),
{
    content: "Check that the image has the correct filter",
    trigger: ".snippet-option-ImageTools we-select:contains('Filter') we-toggler:contains('Blur')",
    isCheck: true,
}, {
    content: "Click on move to next",
    trigger: ".snippet-option-GalleryElement we-button[data-position='next']",
}, {
    content: "Check that the moved image still has the correct filter",
    // FIXME somehow checking what the editor panel shows here is not reliable
    // unless you add a big delay before checking.
    trigger: "iframe .s_image_gallery .carousel-item.active img[data-index='1'][data-gl-filter='blur']",
    isCheck: true,
}]);
