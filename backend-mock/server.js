const express = require('express');
const cors = require('cors');
const axios = require('axios');
const cheerio = require('cheerio');
const { v4: uuidv4 } = require('uuid');

const app = express();
const PORT = 5002;

// Middleware
app.use(cors());
app.use(express.json());

// In-memory storage for job applications
let jobs = [];

// Helper function to detect work type
function detectWorkType(text) {
    if (!text) return 'Remote';
    const lowerText = text.toLowerCase();

    if (lowerText.includes('hybrid')) return 'Hybrid';
    if (lowerText.includes('on-site') || lowerText.includes('onsite') || lowerText.includes('office')) return 'On-site';
    if (lowerText.includes('remote') || lowerText.includes('work from home')) return 'Remote';

    return 'Remote';
}

// Helper function to extract text safely
function extractText(selector, $ = null) {
    if (!$) return '';
    const element = $(selector);
    return element.text().trim().substring(0, 1000);
}

// Note: Direct web scraping is blocked by most job sites
// Instead, we provide a quick template for users to fill in
function getDefaultJobForm() {
    return {
        jobTitle: '',
        companyName: '',
        description: 'Paste the job description here or visit the link to copy it',
        location: '',
        workType: 'Remote'
    };
}

// Routes

// GET all jobs
app.get('/api/jobs', (req, res) => {
    res.json(jobs);
});

// GET single job
app.get('/api/jobs/:id', (req, res) => {
    const job = jobs.find(j => j.id === req.params.id);
    if (!job) return res.status(404).json({ message: 'Job not found' });
    res.json(job);
});

// POST create job
app.post('/api/jobs', (req, res) => {
    const { jobTitle, companyName, jobLink, description, location, workType, applicationStatus, notes } = req.body;

    if (!jobTitle || !companyName) {
        return res.status(400).json({ message: 'Job title and company name are required' });
    }

    const job = {
        id: uuidv4(),
        jobTitle,
        companyName,
        jobLink: jobLink || '',
        description: description || '',
        location: location || '',
        workType: workType || 'Remote',
        applicationStatus: applicationStatus || 'Applied',
        notes: notes || '',
        createdDate: new Date(),
        updatedDate: new Date()
    };

    jobs.push(job);
    res.status(201).json(job);
});

// PUT update job
app.put('/api/jobs/:id', (req, res) => {
    const job = jobs.find(j => j.id === req.params.id);
    if (!job) return res.status(404).json({ message: 'Job not found' });

    Object.assign(job, req.body, {
        id: job.id,
        createdDate: job.createdDate,
        updatedDate: new Date()
    });

    res.json(job);
});

// DELETE job
app.delete('/api/jobs/:id', (req, res) => {
    const index = jobs.findIndex(j => j.id === req.params.id);
    if (index === -1) return res.status(404).json({ message: 'Job not found' });

    jobs.splice(index, 1);
    res.status(204).send();
});

// POST extract job info from URL
app.post('/api/jobs/extract', (req, res) => {
    const { url } = req.body;

    if (!url) {
        return res.status(400).json({ message: 'URL is required' });
    }

    console.log('Extraction request for:', url);

    // Return a template for the user to fill in
    // Job sites block automated scraping, so we provide a form template
    res.json({
        jobTitle: '',
        companyName: '',
        description: 'Visit the job link to copy the job description and paste it here',
        location: '',
        workType: 'Remote'
    });
});

// Start server
app.listen(PORT, () => {
    console.log(`
╔════════════════════════════════════════════════════════════╗
║     Job Application Tracker - Backend Server              ║
║                                                            ║
║  Server running on: http://localhost:${PORT}               ║
║                                                            ║
║  Available endpoints:                                     ║
║  - GET    /api/jobs                                       ║
║  - GET    /api/jobs/:id                                   ║
║  - POST   /api/jobs                                       ║
║  - PUT    /api/jobs/:id                                   ║
║  - DELETE /api/jobs/:id                                   ║
║  - POST   /api/jobs/extract                               ║
║                                                            ║
║  Frontend: http://localhost:4200                          ║
║                                                            ║
║  Press Ctrl+C to stop                                     ║
╚════════════════════════════════════════════════════════════╝
  `);
});
